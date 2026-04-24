import http from "k6/http";
import { check, sleep } from "k6";

const baseUrl = __ENV.BASE_URL || "http://localhost:5228";

export const options = {
  vus: 2,
  duration: "30s",
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<500"],
  },
};

export default function () {
  const response = http.get(`${baseUrl}/api/movies`);

  check(response, {
    "smoke status is 200": (r) => r.status === 200,
    "smoke payload is array": (r) => Array.isArray(r.json()),
  });

  sleep(1);
}
