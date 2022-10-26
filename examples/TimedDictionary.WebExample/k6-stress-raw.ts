// k6 run --vus 1000 --duration 30s examples/TimedDictionary.WebExample/k6-stress-raw.ts

import http from 'k6/http';

export default function () {
    var value = Math.ceil(Math.random() * 1);

    // Activate to test bundling
    http.get(`http://localhost:5000/stress/bundle/${value}`);

    // Activate to test without bundling
    // http.get(`http://localhost:5000/stress/raw/${value}`);
}