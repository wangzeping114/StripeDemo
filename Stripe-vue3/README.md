# Vue 3 Stripe支付演示

这是一个使用Vue 3和Stripe实现的支付演示项目。

## 功能特点

- 集成Stripe支付系统
- 支持信用卡支付
- 实时错误提示
- 响应式设计

## 安装步骤

1. 克隆项目
```bash
git clone [项目地址]
cd vue3-stripe
```

2. 安装依赖
```bash
npm install
```

3. 配置Stripe
- 在 [Stripe Dashboard](https://dashboard.stripe.com/) 注册账号
- 获取您的公钥（Publishable Key）
- 在 `src/components/StripePayment.vue` 中替换 `your_publishable_key`

4. 运行项目
```bash
npm run dev
```

## 注意事项

- 这是一个前端演示项目，实际使用时需要配合后端API
- 请确保在生产环境中使用HTTPS
- 不要在前端代码中暴露Stripe私钥

## 测试卡号

可以使用以下测试卡号进行测试：
- 卡号：4242 4242 4242 4242
- 有效期：任意未来日期
- CVC：任意三位数
- 邮编：任意五位数 