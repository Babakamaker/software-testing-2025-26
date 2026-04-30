import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '1m', target: 10 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_failed: ['rate<0.02'],
    http_req_duration: ['p(95)<750'],
    checks: ['rate>0.98'],
  },
};

const baseUrl = __ENV.BASE_URL || 'http://localhost:5067';

export default function () {
  const response = http.get(`${baseUrl}/api/orders`);

  check(response, {
    'orders endpoint returns 200': (r) => r.status === 200,
  });

  sleep(1);
}
