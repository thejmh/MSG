# Use the official Node.js 22 alpine image as base
FROM node:22-alpine

# Set working directory
WORKDIR /app

# Copy package.json
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy application source code
COPY . .

# Expose the port Angular runs on
EXPOSE 3000

# Start the application in development mode
CMD ["npm", "run", "dev"]
