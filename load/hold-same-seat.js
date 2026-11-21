// k6: 1000 пользователей одновременно жмут на ОДНО место. Ожидание: ровно 1 × 201, остальные 409/429.
// Запуск: k6 run load/hold-same-seat.js   (API должен быть запущен на http://localhost:5042)
import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

const created = new Counter('hold_201');
const conflict = new Counter('hold_409');
const limited = new Counter('hold_429');

const BASE = __ENV.BASE_URL || 'http://localhost:5042';
const TRIP = '11111111-1111-1111-1111-111111111111';
const SEAT = Number(__ENV.SEAT || 7);

export const options = {
  scenarios: {
    burst: { executor: 'per-vu-iterations', vus: 1000, iterations: 1, maxDuration: '60s' },
  },
  thresholds: { hold_201: ['count==1'] }, // двойная продажа = провал теста
};

function uuid() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
    const r = (Math.random() * 16) | 0;
    return (c === 'x' ? r : (r & 0x3) | 0x8).toString(16);
  });
}

export default function () {
  const res = http.post(`${BASE}/api/trips/${TRIP}/wagons/2/holds`, JSON.stringify({ seats: [SEAT] }), {
    headers: { 'Content-Type': 'application/json', 'X-Debug-User': uuid() },
  });
  if (res.status === 201) created.add(1);
  else if (res.status === 409) conflict.add(1);
  else if (res.status === 429) limited.add(1);
  check(res, { 'expected status': r => [201, 409, 429].includes(r.status) });
}
