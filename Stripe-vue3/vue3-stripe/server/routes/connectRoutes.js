const express = require('express');
const router = express.Router();
const connectController = require('../controllers/connectController');

// 创建Connect账户
router.post('/create-account', connectController.createAccount);

// 创建账户链接
router.post('/create-account-link', connectController.createAccountLink);

// 获取账户状态
router.get('/account-status', connectController.getAccountStatus);

module.exports = router; 