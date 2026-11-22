require('dotenv').config();
const express = require('express');
const cors = require('cors');
const morgan = require('morgan');
const { createProxyMiddleware } = require('http-proxy-middleware');

const app = express();
const PORT = process.env.PORT || 8085;

// Middleware - Logger
app.use(morgan('dev'));

// CORS configuration supporting credentials and all local dev origins
const corsOptions = {
  origin: (origin, callback) => {
    // allow requests with no origin (like mobile apps, curl, etc.)
    if (!origin) return callback(null, true);
    if (
      origin.startsWith('http://localhost') ||
      origin.startsWith('http://127.0.0.1') ||
      origin === process.env.FRONTEND_URL
    ) {
      return callback(null, true);
    }
    return callback(null, true); // Permissive in dev
  },
  credentials: true,
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization', 'apikey']
};

app.use(cors(corsOptions));
app.options('*', cors(corsOptions)); // Handle preflight for all routes

app.use(express.json());

// Health endpoints
app.get('/', (req, res) => {
  res.json({
    service: 'Agri-Connect API Gateway',
    status: 'Running',
    port: PORT,
    adminService: process.env.ADMIN_SERVICE_URL || 'http://localhost:5001'
  });
});

app.get('/health', (req, res) => {
  res.json({ status: 'UP', port: PORT });
});

// Proxy options helper
const createServiceProxy = (target, name) =>
  createProxyMiddleware({
    target,
    changeOrigin: true,
    // Preserve full path so backend receives /api/auth/login, etc.
    pathRewrite: (path, req) => req.originalUrl,
    on: {
      error: (err, req, res) => {
        console.error(`[Gateway Error] Proxy to ${name} (${target}) failed:`, err.message);
        if (!res.headersSent) {
          res.status(502).json({
            error: 'Bad Gateway',
            message: `${name} is currently unreachable at ${target}.`,
            details: err.message
          });
        }
      },
      proxyReq: (proxyReq, req) => {
        // Forward body for POST/PUT when body was parsed by express.json()
        if (req.body && Object.keys(req.body).length > 0) {
          const bodyData = JSON.stringify(req.body);
          proxyReq.setHeader('Content-Type', 'application/json');
          proxyReq.setHeader('Content-Length', Buffer.byteLength(bodyData));
          proxyReq.write(bodyData);
        }
      }
    }
  });

// Routes
// 1. Auth routes -> Admin Service (:5001)
app.use('/api/auth', createServiceProxy(process.env.ADMIN_SERVICE_URL || 'http://localhost:5001', 'Admin Service'));

// 2. Admin routes -> Admin Service (:5001)
app.use('/api/admin', createServiceProxy(process.env.ADMIN_SERVICE_URL || 'http://localhost:5001', 'Admin Service'));

// 3. Other services
app.use('/api/farmers', createServiceProxy(process.env.FARMER_SERVICE_URL || 'http://localhost:5002', 'Farmer Service'));
app.use('/api/officers', createServiceProxy(process.env.OFFICER_SERVICE_URL || 'http://localhost:5003', 'Officer Service'));
app.use('/api/suppliers', createServiceProxy(process.env.SUPPLIER_SERVICE_URL || 'http://localhost:5004', 'Supplier Service'));

// 404 Handler
app.use((req, res) => {
  res.status(404).json({
    error: 'Not Found',
    message: `No route found for ${req.method} ${req.originalUrl} on API Gateway.`
  });
});

app.listen(PORT, () => {
  console.log(`===============================================`);
  console.log(`  Agri-Connect API Gateway running on port ${PORT}`);
  console.log(`  Auth routes -> ${process.env.ADMIN_SERVICE_URL || 'http://localhost:5001'}`);
  console.log(`===============================================`);
});
