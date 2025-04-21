# Stripe Connect Custom模式集成指南

本文档提供了如何在Vue.js应用中集成Stripe Connect的Custom模式的详细指南。Custom模式允许平台完全控制用户体验，并代表连接账户处理所有交互。

## 功能概述

- 创建Stripe Connect Custom账户
- 引导用户完成账户验证流程
- 检查账户验证状态
- 控制资金流向和付款流程

## 先决条件

1. Stripe账户（在[Stripe Dashboard](https://dashboard.stripe.com/)注册）
2. 启用Stripe Connect功能（在[Connect设置](https://dashboard.stripe.com/settings/connect)中配置）
3. 获取API密钥（在[API密钥设置](https://dashboard.stripe.com/apikeys)中获取）

## 项目结构

```
├── server/                  # 后端Express服务器
│   ├── controllers/         # 控制器
│   │   └── connectController.js  # Connect相关API处理
│   ├── routes/              # 路由定义
│   │   └── connectRoutes.js      # Connect路由
│   ├── index.js             # 服务器入口文件
│   ├── package.json         # 服务器依赖
│   └── .env                 # 环境变量配置
├── src/                     # 前端Vue应用
│   ├── components/          # Vue组件
│   │   └── ConnectOnboarding.vue  # Connect注册组件
│   └── router/              # Vue路由
│       └── index.js         # 路由配置
└── CONNECT_CUSTOM_README.md # 本文档
```

## 安装说明

### 1. 服务器设置

1. 进入server目录并安装依赖：
   ```bash
   cd server
   npm install
   ```

2. 创建.env文件并配置Stripe API密钥：
   ```
   cp .env.example .env
   ```
   编辑.env文件，填入您的Stripe API密钥：
   ```
   STRIPE_SECRET_KEY=sk_test_your_test_key
   STRIPE_PUBLISHABLE_KEY=pk_test_your_test_key
   ```

3. 启动服务器：
   ```bash
   npm run dev
   ```
   服务器将在http://localhost:5062上运行。

### 2. 客户端设置

1. 在项目根目录安装依赖：
   ```bash
   npm install
   ```

2. 启动Vue开发服务器：
   ```bash
   npm run dev
   ```
   客户端应用将在http://localhost:3000上运行。

## 使用方法

1. 打开浏览器访问http://localhost:3000
2. 在主页点击"成为商户"选项
3. 在Connect注册页面点击"开始注册"
4. 按照Stripe提供的表单完成账户验证流程
5. 验证完成后，您可以在ConnectOnboarding组件中看到账户状态

## 实现细节

### 服务器端实现

服务器端实现了三个主要的API端点来支持Stripe Connect Custom功能：

1. **创建Connect账户**：
   ```javascript
   POST /api/Connect/create-account
   ```
   创建一个新的Stripe Connect Custom账户。

2. **创建账户链接**：
   ```javascript
   POST /api/Connect/create-account-link
   ```
   生成一个URL，引导用户完成账户验证流程。

3. **获取账户状态**：
   ```javascript
   GET /api/Connect/account-status?accountId={accountId}
   ```
   检查Connect账户的验证状态。

### 客户端实现

客户端使用Vue.js实现了一个ConnectOnboarding组件，用于：

1. 创建Connect账户
2. 引导用户完成验证流程
3. 检查和显示账户状态

## 注意事项

- **安全性**：确保您的Stripe密钥安全，永远不要暴露在客户端代码中。
- **测试模式**：在开发过程中使用测试模式API密钥。
- **IP地址**：Custom账户验证需要用户的IP地址，请确保正确收集。
- **合规要求**：根据不同国家/地区的法规，验证要求可能有所不同。确保您的集成满足目标市场的要求。

## 常见问题

1. **账户验证失败怎么办？**
   检查账户状态API返回的requirements字段，了解哪些信息缺失或需要更新。

2. **如何接收付款到Custom账户？**
   需要使用Stripe的支付API，并使用Stripe-Account头部指定接收付款的账户ID。

3. **如何处理多个国家/地区的账户？**
   在创建账户时指定相应的country参数，并确保您的平台符合该国家/地区的要求。

## 资源链接

- [Stripe Connect 文档](https://docs.stripe.com/connect)
- [Custom 账户指南](https://docs.stripe.com/connect/custom-accounts)
- [账户验证要求](https://docs.stripe.com/connect/required-verification-information)
- [Connect API 参考](https://docs.stripe.com/api/connect) 