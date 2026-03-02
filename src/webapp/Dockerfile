FROM node:18 AS build
WORKDIR /app

RUN npm install -g bower gulp-cli

# Copy dependency manifests for layer caching
COPY package.json package-lock.json bower.json .bowerrc ./
# Allow bower to run as root in Docker
RUN node -e "const fs=require('fs'); const b=JSON.parse(fs.readFileSync('.bowerrc','utf8')); b.allow_root=true; fs.writeFileSync('.bowerrc',JSON.stringify(b));"

# Install npm deps (triggers bower install via the "install" lifecycle script)
RUN npm install

# Copy source and build
COPY . .
RUN gulp build-prod

# Runtime stage
FROM node:18-slim AS runtime
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm install --omit=dev --ignore-scripts

COPY src/server ./src/server
COPY --from=build /app/build ./build

EXPOSE 7203
ENV NODE_ENV=build
CMD ["node", "src/server/app.js"]
