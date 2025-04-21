const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
const connectRoutes = require('./routes/connectRoutes');
const stripe = require('stripe')(process.env.STRIPE_SECRET_KEY);

const app = express();
const PORT = process.env.PORT || 5062;

// 中间件
app.use(cors());
app.use(bodyParser.json());

// 路由
app.use('/api/Connect', connectRoutes);

// 健康检查路由
app.get('/health', (req, res) => {
  res.status(200).json({ status: 'ok' });
});

// 启动服务器
app.listen(PORT, () => {
  console.log(`服务器运行在端口 ${PORT}`);
});

module.exports = app; 