# Frontend Dockerfile with hot reload support
FROM node:18-alpine AS base
WORKDIR /app

# Install dependencies
FROM base AS dependencies
COPY package*.json ./
RUN npm ci

# Development stage with hot reload
FROM dependencies AS development
WORKDIR /app
COPY . .

# Expose Angular dev server port
EXPOSE 4200

# Enable hot reload with polling (works better in Docker)
CMD ["npm", "run", "start:docker"]

# Production build stage
FROM dependencies AS build
WORKDIR /app
COPY . .
RUN npm run build --configuration=production

# Production stage with nginx
FROM nginx:alpine AS production
COPY --from=build /app/dist/frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]