import http from "k6/http";
import { check, sleep } from "k6";

const baseUrl = __ENV.BASE_URL || "http://localhost:5228";

export const options = {
  scenarios: {
    stress_reviews: {
      executor: "ramping-vus",
      stages: [
        { duration: "30s", target: 10 },
        { duration: "30s", target: 30 },
        { duration: "30s", target: 60 },
        { duration: "30s", target: 90 },
        { duration: "30s", target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.10"],
    http_req_duration: ["p(95)<800"],
  },
};

export default function () {
  const movieId = 1 + Math.floor(Math.random() * 2500);
  const userId = 10000 + ((__VU * 1000) + __ITER);
  const score = 1 + Math.floor(Math.random() * 10);

  const payload = JSON.stringify({
    userId,
    score,
    comment: `Stress scenario review comment ${__VU}-${__ITER} with enough details.`,
  });

  const response = http.post(`${baseUrl}/api/movies/${movieId}/reviews`, payload, {
    headers: { "Content-Type": "application/json" },
  });

  check(response, {
    "stress accepted or validated": (r) => [201, 400, 409].includes(r.status),
    "stress response has body": (r) => r.body && r.body.length > 0,
  });

  sleep(0.5);
}
