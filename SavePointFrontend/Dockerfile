FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
ARG VITE_API_BASE_URL=http://localhost:8080
ARG VITE_TOP_GAMES_URL=
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_TOP_GAMES_URL=$VITE_TOP_GAMES_URL
RUN npm run build

FROM node:20-alpine AS runtime
WORKDIR /app
RUN npm i -g serve
COPY --from=build /app/dist ./dist

EXPOSE 4173
CMD ["serve", "-s", "dist", "-l", "4173"]