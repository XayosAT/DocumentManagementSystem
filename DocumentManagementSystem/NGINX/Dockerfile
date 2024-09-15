# Use the official Nginx image
FROM nginx:latest

# Copy the custom Nginx configuration file
COPY nginx.conf /etc/nginx/nginx.conf

# Copy static files (HTML, CSS, JS) to the Nginx HTML directory
COPY index.html /usr/share/nginx/html/
