/**
 * Single entry point for the whole backend (the API Gateway). The Angular app never talks to
 * AuthService or ProductService directly - the gateway routes /api/auth/*, /api/products/* and
 * /odata/* to the right microservice.
 */
export const API_BASE_URL = 'http://localhost:5000';
