FROM node:22.16-alpine3.22 AS build-stage
ARG API_HOST_URL

COPY . /app
WORKDIR /app

# env variables are setted during build
ENV VITE_API_HOST_URL=$API_HOST_URL

RUN npm ci
RUN npm run build

FROM nginx:1.29-alpine3.22
COPY nginx.conf /etc/nginx/conf.d/default.conf
WORKDIR /usr/share/nginx/html
COPY --from=build-stage /app/dist .
