# Multi-stage Dockerfile to handle WebAPI and WebApp in a single image

# Build webapp
FROM node:22.16-alpine3.22 AS webapp-build
# API URL is required in advance
ARG API_URL

WORKDIR /app
COPY client/webapp ./
ENV VITE_WEBAPI_URL=$API_URL
RUN npm ci
RUN npm run build

# Build webapi
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS webapi-build
WORKDIR /app
COPY PoultryFarmManager.sln ./
COPY server/ ./server

RUN dotnet restore PoultryFarmManager.sln && \
    dotnet publish -c Release -o published server/PoultryFarmManager.WebAPI/PoultryFarmManager.WebAPI.csproj

# Runtime image - includes both .NET and Nginx
FROM nginx:1.29-alpine3.22

# Install ASP.NET Core runtime
RUN apk update --no-cache && \
    apk add --no-cache aspnetcore8-runtime

# Copy applications
COPY --from=webapi-build /app/published ./webapi/
WORKDIR /usr/share/nginx/html
COPY --from=webapp-build /app/dist .
# Configure Nginx
COPY client/webapp/nginx.conf /etc/nginx/conf.d/default.conf

# Environment variables
EXPOSE 80

# Default cmd from nginx base image
