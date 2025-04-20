# Stripe支付示例 (Vue 3)

这是一个使用Vue 3和Stripe.js构建的简单支付演示应用。

## 功能特点

- Vue 3 组合式API
- Stripe支付集成
- 响应式UI设计
- 简洁的用户体验
- 后端API调用处理支付
- 完整的订单和用户信息支持

## 项目设置

### 安装依赖
```sh
npm install
```

### 配置Stripe
打开`src/components/StripePayment.vue`文件，将`pk_test_your_publishable_key`替换为您的Stripe公钥。

### 后端API要求
此项目需要以下后端API端点：
- `http://localhost:5062/api/Payment/create-payment-intent` - 创建支付意向并返回客户端密钥
- `http://localhost:5062/api/Payment/update-order-status` - 更新订单状态

如需修改后端地址，请编辑`src/components/StripePayment.vue`文件中的`baseApiUrl`常量。

### 开发环境运行
```sh
npm run dev
```

### 生产环境构建
```sh
npm run build
```

## 后端API格式

### 创建支付意向API
```
POST http://localhost:5062/api/Payment/create-payment-intent
Content-Type: application/json

{
  "amount": 1999,
  "currency": "usd",
  "orderId": "ORD1653890112233",
  "description": "商品购买",
  "userId": "user123",
  "userName": "测试用户",
  "email": "test@example.com"
}
```

响应:
```json
{
  "clientSecret": "pi_xxxxx_secret_xxxxx"
}
```

### 更新订单状态API
```
POST http://localhost:5062/api/Payment/update-order-status
Content-Type: application/json

{
  "paymentIntentId": "pi_xxxxx",
  "amount": 1999,
  "currency": "usd",
  "orderId": "ORD1653890112233",
  "description": "商品购买",
  "userId": "user123",
  "userName": "测试用户",
  "email": "test@example.com",
  "paymentStatus": "succeeded",
  "paymentMethod": "card",
  "transactionDate": "2023-09-01T12:34:56.789Z",
  "cardLast4": "4242"
}
```

## 注意事项
- 需要配置后端服务来处理支付请求
- 确保后端服务器在 http://localhost:5062 运行
- 在生产环境中使用时，请确保遵循PCI合规标准
- 请勿在生产环境中使用测试密钥
- 在实际应用中，应当从用户会话或表单中获取用户信息，而非硬编码
