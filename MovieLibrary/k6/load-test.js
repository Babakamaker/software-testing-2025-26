import http from "k6/http";
import { check, sleep } from "k6";

const baseUrl = __ENV.BASE_URL || "http://localhost:5228";
const genres = ["Action", "Drama", "Comedy", "Thriller", "SciFi", "Romance"];

export const options = {
  scenarios: {
    normal_load: {
      executor: "ramping-vus",
      stages: [
        { duration: "30s", target: 10 },
        { duration: "30s", target: 30 },
        { duration: "30s", target: 50 },
        { duration: "30s", target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.02"],
    http_req_duration: ["p(95)<500"],
  },
};

export default function () {
  const genre = genres[Math.floor(Math.random() * genres.length)];
  const year = 1980 + Math.floor(Math.random() * 46);
  const response = http.get(`${baseUrl}/api/movies?genre=${genre}&year=${year}`);

  check(response, {
    "load status is 200": (r) => r.status === 200,
    "load payload is array": (r) => Array.isArray(r.json()),
  });

  sleep(1);
}
