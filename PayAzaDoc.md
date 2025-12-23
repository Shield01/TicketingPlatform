# Overview
Get Started with
Payaza Documentation
You want to integrate with our systems? Take a look at our checkout to get started.

No need to code to start receiving payments. Start by taking a look at the No Code Guide.

Lets get you started
Explore Payaza’s range of services.

### Make Payments
Learn how you can make payments via card and bank transfer

### Accept Payments
Learn how to create payment links, static virtual accounts and more


### Create a Payaza Branch
Learn what branches are, how to create them, and when to create them

### Use Your API Keys
Learn how you retrieve and use your Payaza accounts API keys

### Use Your Payaza Account
Learn how to make payouts and view transactions on your Payaza Account

### Verify Your Business
Having issues with your Payaza account verification?


# Test vs Live Environment
## Payaza Application Environments

The Payaza API offers functionality in two distinct environments: Test and Live. It's important to understand the purpose of each environment to ensure proper integration and data handling.

Test Environment:

Designed for testing and development purposes only.

Utilizes simulated data to mimic real-world scenarios.

Ideal for exploring API functionalities and building integrations before deploying to production.

Live Environment:

Facilitates real integrations with your application.
Processes actual business data and transactions.
Requires a separate set of credentials compared to the test environment.

**Note:** 

Data and credentials (business IDs, API keys, etc.) are unique to each environment. Information used in the test environment won't work in the live environment, and vice versa

Test and Live Environments are all encompassed in the Payaza Dashboard, allowing users to switch between environments. You'll need to use the toggle on the Dashboard

# Business APIs and Webhooks
## Introduction

Generally, when you request an API endpoint, you expect a near-immediate response. However, some requests may take longer to process, leading to potential timeout errors. To prevent this, the API may return a pending response instead. Since your records need to be updated with the final state of the request, you have two options:

Transaction Status Query (TSQ) : Periodically make a request to the API to check for updates until the final status is available.

Webhooks : Set up a webhook URL to listen for events, allowing the API to automatically send updates to your server when the request is complete.

Both methods ensure that your records are kept up to date without running into timeout issues.

## About webhooks

Webhooks allow you to set up a notification system that sends real-time updates to your server when specific events occur within the Payazaa API. By configuring webhooks, you can automatically receive information about various actions, such as successful payments, refunds, or subscription updates, without constantly polling the API. This ensures you stay informed and can take immediate action based on the events, leading to more efficient and responsive integrations.

## Access your API Keys
Getting your API keys is essential for integrating with Payaza’s services. Every account is equipped with a public API key. This key is used to identify your account with Payaza and is safe to use in client-side code. It allows you to initiate transactions and other operations while ensuring that they are linked to your specific account.

This API key is available in two modes:

Live Mode: These keys are used for real transactions and interactions with real customers.

Test Mode: These keys are used for testing and development purposes, allowing you to simulate transactions and other operations without affecting real data or customers.

By properly managing and using these API keys, you can securely and effectively integrate Payaza’s services into your applications.


# Dev Guide
## Authentication

Payaza authenticates your API requests using your account's API keys, Add your Base 64 encoded API key in the Authorization header with the Payaza Prefix

Authorization: Payaza [Add-your-Base 64 encoded API key]

## Sample
Authorization: Payaza UFo4748S0xJVkUtSJDS5RThDQzEtQjAzMS00RUNBLTgwOTctRUVCMjA5NzJENTY0

If you do not include your base 64 encoded key when making an API request or use one that is incorrect, we will return this error:
```json 
{
  "message": "Authentication failed",
  "status": false,
  "retry_count": 0
}
```

## API Keys
API keys can be managed from your Payaza Dashboard. Payaza uses a single type of API key which is the Public Key.

Public Keys: Can be shared with your developers or trustworthy team members.
API requests exist in either test or live mode, one mode’s data cannot affect the other.

We have provided a toggle button to help you switch easily between both modes and an indicator to help identify what mode you are in.


Note

Only use the test API Key for test development purposes. Payments in test mode are not processed or settled.

### Retrieving your API Keys
Your API keys are available on your dashboard. Follow the steps below to access them:

Login to your Payaza dashboard
Click on the Settings option which is located on the left side-bar of the dashboard
Click the API and Webhooks option from the dropdown menu
Click on the View Key Button to reveal your key which can also be copied
Please switch to Test Mode to retrieve your test key


# Webhooks
Webhooks receive messages based on events triggered in the system, they are an important part of your payment integration. Webhooks are responsible for notifying you about events that happen on your accounts, such as pending, successful, or failed transactions.

You can use Webhooks to configure and receive notifications when a specific event occurs. When one of these events is triggered, we send a POST payload in JSON which contains the details about the event, to the webhook's configured URL. Setting up a webhook allows us to notify you when these payments are completed. Payaza sends webhooks for:

Collections
Transfers

## When to use webhooks
Webhooks are event-based and enable real-time updates of third-party systems as they are triggered and sent out immediately when specific events related to the transaction happen. They're useful for methods and events that occur outside your application's control, such as:

Getting paid via mobile money or USSD
Pending payment transactions to successful
These are all non-simultaneous actions—they are not controlled by your application, so you won't know when they are completed unless we notify you or you check later. You can set up Webhooks from your Payaza Dashboard and configure separate URLs for Live mode and Test mode.


    Note
    Your webhook notification includes an authentication header with your public key encoded in base64 format. This can be   used to validate that a webhook notification was sent from us

    **Idempotency:** Process transactions based on their unique transaction_reference to avoid duplicate actions.

### Validating Webhook Payloads with HMAC
Our webhook notifications include a header called x-payaza-signature which is an HMAC SHA512 signature generated from the event payload using your secret key.
To ensure the integrity and authenticity of webhook events:

- Always validate the ⁠ x-payaza-signature ⁠ before processing any event.
- Compute an HMAC SHA512 hash using the received payload and your secret key. The secret key doesn't need to be base64 encoded
- Compare the generated hash with the ⁠ x-payaza-signature ⁠ in the request header.
- Ensure that the event is processed only if the computed hash matches the received signature

### HMAC Sample
```javascript
Sample


const crypto = require('crypto');

// Define the request body (ensure it's exactly the same as in cURL)
const requestBody = ⁠ {{webhookNotificationBody}};

// Secret key used for hashing
const secretKey = ""{{secretKey}}";

// Expected signature from webhook header
const predefinedSignature = "{{predefinedKey}}";


// Function to generate HMAC-SHA512 signature
function hmacSHA512(data, secretKey) {
  return crypto.createHmac('sha512', secretKey) // Use SHA512
               .update(data, 'utf8') // Encode as UTF-8
               .digest('base64'); // Encode output as Base64
}

// Generate Computed signature
const computedSignature = hmacSHA512(requestBody, secretKey);

// Compare computed signature with predefined signature
if (computedSignature === predefinedSignature) {
  console.log("✅ SIGNATURE MATCHED SUCCESSFULLY!");
} else {
  console.error("❌ Signature Mismatch. Please check your credentials.");
}
```

### How To Set Up Your Webhook URL
- Login to your Payaza dashboard
- Click on the Settings option which is located on the left side-bar of the dashboard
- Click on the Developers option from the dropdown menu.
- Navigate to add your respective webhook URL
- Select the Update Webhooks button to update your webhook URLs

## Payaza Webhook Notification Samples
### Payout

```json 
Successful Transfer


{
  "transaction_reference": "PTSA1220246261518348000",
  "transaction_type": "DEBIT",
  "transaction_status": "NIP_SUCCESS",
  "transaction_fee": 10.0,
  "amount_received": 20.0,
  "sent_to": {
      "account_name": "John Doe",
      "account_number": "1234567890",
      "bank_name": "EASTWE BANK"
  },
  "initiated_date": "2024-06-26 16:44:08.718",
  "current_status_date": "2024-06-26 16:44:08.718",
  "is_reversed": false,
  "response_message": "Approved or Completely Successful",
  "response_code": "00",
  "currency": "NGN",
  "country": "NGA",
  "session_id": "999999213419011273456123134124"
}
```

```json
Failed Transfer


{
"narration": "PTSA1220246261518348001",
"transaction_reference": "PTSA1220246261518348001",
"transaction_type": "DEBIT",
"transaction_status": "NIP_FAILURE",
"transaction_fee": 100,
"amount_received": 50000,
"sent_to": {
  "account_name": "John Doe",
  "account_number": "1234567890",
  "bank_name": "EASTWE BANK"
},
"initiated_date": "2024-06-26 16:44:08.718",
"current_status_date": "2024-06-26 16:44:08.718",
"is_reversed": true,
"response_message": "Invalid Account",
"response_code": "07",
"currency": "NGN",
"country": "NGA"
}
```
### Collections

``` json
Through Virtual Account


{
  "transaction_reference": "testingamount",
  "transaction_status": "Funds Received",
  "virtual_account_number": "7000155015",
  "transaction_fee": 100,
  "amount_received": 120,
  "initiated_date": "2024-01-03 10:19:34",
  "current_status_date": "2024-01-03 10:19:34",
  "received_from": {
    "account_name": "CALEB CHARLES",
    "account_number": "08093414132",
    "bank_name": "Bank 78"
  },
  "merchant_reference": "testingamount",
  "channel": "VirtualAccount",
  "currency_code": "NGN",
  "branch": false,
  "session_id": "10000424090951185519086370217",
  "status": "Completed"
}
```

```json
Card Collection


{
"transaction_reference": "DOVMAR123123",
"transaction_status": "Funds Received",
"transaction_fee": 0.05,
"amount_received": 1.00,
"initiated_date": "2024-06-25 09:47:37",
"current_status_date": "2024-06-25 09:47:37",
"received_from": {
  "account_name": "John Smith",
  "bank_name": "MASTERCARD"
},
"status": "Completed",
"session_id": "435319842451",
"channel": "Card",
"branch": false,
"currency_code": "NGN"
}
```

```json
MOMO Collection


{
"transaction_reference": "LAX1234",
"transaction_status": "Funds Received",
"virtual_account_number": "",
"transaction_fee": 1,
"amount_received": 50.5,
"initiated_date": "2024-09-08 20:24:09",
"current_status_date": "2024-09-08 20:24:48",
"received_from": {
    "account_name": "John Doe",
    "account_number": "233123456789",
    "bank_name": "N/A"
},
"channel": "KE_MOBILEMONEY",
"currency_code": "KES",
"branch": false,
"session_id": "P-C-202498-779AB97963",
"status": "Completed"
}
```

# Test Cards
The following test cards can be used to perform test transactions on our platform. These cards are only valid in Test Mode and will not work in production.

###Test Card Details

| **Card Type** | **Card Number**     | **3D Secure (3DS)** | **Expiry Date** | **CVV** |
|:--------------:|:-------------------:|:--------------------:|:---------------:|:-------:|
| **Visa**       | 4508750015741019    | True                 | 01/39           | 100     |
| **Mastercard** | 5123450000000008    | True                 | 01/39           | 100     |
| **Mastercard** | 5111111111111118    | False                | 01/39           | 100     |

### Simulated Expiry Date Responses

The expiry date used in test transactions determines the system’s response.

| **Expiry Date** | **Response**  |
|:--------------:|:-------------------:|
| **01/39**       | APPROVED    |
| **05/39** | DECLINED      |
| **04/27** | EXPIRED_CARD    |
| **08/28**       | TIMED_OUT    |
| **01/37** | ACQUIRER_SYSTEM_ERROR      |
| **02/37** | UNSPECIFIED_FAILURE    |
| **05/37** | UNKNOWN    |

These test scenarios help simulate different transaction outcomes, allowing merchants to validate their integration effectively.

# Errors
These are some of the major errors that can be encountered while integrating with Payaza and how to fix them.

### Authorization
    
    {
      "message": "Authentication failed",
      "status": false,
      "retry_count": 0
    }
                
            

❌ Error :
This error occurs if you do not provide your API key or the provided is wrong or there is an error in one or more of the fields.


✅ Fix :
Copy the key from your dashboard, encode it to base 64 and paste in the Authentication header with the Payaza Prefix

    AUTHORIZATION
    Sample

    {
      "Authorization": "Payaza UFo4748S0xJVkUtSJDS5RThDQzEtQjAzMS00RUNBLTgwOTctRUVCMjA5NzJENTY0"
    }

### Transfers 

```json
{
  "response_code": 500,
  "response_message": "Payout amount is less than total credit amount"
}
```
                
            
❌ Error : The credit amount is greater than the Payout amount.

✅ Fix : Refer to the parameters, and reduce the amount accordingly before retrying the transaction.

--- 
                
```json
{
  "response_code": 500,
  "response_message": "Insufficient Balance"
}
```               
            

❌ Error : This error occurs when the payout amount is greater than your available balance.

✅ Fix : Top up your payaza account or reduce the amount you are paying out and retry the transaction

--- 
```json
{
  "response_code": 500,
  "response_message": "Invalid transaction pin",
  "response_content": {
    "message": "Invalid transaction Pin",
    "retry_count": 1
  }
}
```
                
❌ Error : This occurs when you have passed an incorrect PIN.

✅ Fix : Refer to the parameters, and enter the correct transaction PIN.

---

```json
{
  "response_code": 0,
  "response_message": "Invalid transaction Pin",
  "response_content": {
    "message": "Invalid transaction Pin",
    "status": false,
    "retry_count": 2
  },
  "resp_code": "500"
}
```
                
            

❌ Error : This occurs when 3 incorrect PINs have been passed.

✅ Fix : You have exceeded the number of retries. Kindly reset your PIN on the Payaza Dashboard.
---

```json
{
  "response_code": 0,
  "response_message": "Transaction reference already exists. please use unique reference",
  "resp_code": "X03"
}
```
❌ Error : This occurs when you use the same transaction reference for a transaction. Use a unique transaction reference for each transaction

✅ Fix : Use a unique transaction reference for each transaction

---

```json 
{
  "response_code": 0,
  "response_message": "Bank code is not correct for reference: payazaTest",
  "resp_code": "X02"
}
```

❌ Error : This error occurs when the bank code is incorrect

✅ Fix : Make sure your bank code value is 6 digits and also valid.

---

```json
{
  "response_code": 0,
  "response_message": "Account number can only be numbers",
  "resp_code": "X01"
}
```

❌ Error : This error occurs when the bank account number being used contains other characters or digits.

✅ Fix : Pass a valid account number

---

```json
{
  "response_code": 0,
  "response_message": "Account number must be 10 digits",
  "resp_code": "X01"
}
```

❌ Error : This error occurs when the bank account number is more than 10 digits. Note: This is only for NGN payouts

✅ Fix : Pass a valid account number

---

### Virtual Account

```json
{
  "response_code": 404,
  "response_message": "Transaction not found",
  "response_scontent": {
    "transaction_reference": ""
  }
}
```

❌ Error : This error occurs when a transaction reference doesn't exist

✅ Fix : Fix:Make sure your transaction reference is correct

---


```json               
{
  "response_code": 404,
  "response_message": "Reserved Virtual Account does not exist",
  "response_content": {
    "page": 0,
    "transaction_count": 0,
    "virtual_account_number": "960633617",
    "virtual_account_name": null,
    "virtual_account_status": null,
    "virtual_account_provider_bank": null,
    "transactions": null
  }
}
```

❌ Error : This error occurs if the reserved virtual account number doesn't exist

✅ Fix : Make sure your reserved virtual account number is correct

---

```json
{
  "response_code": 500,
  "response_message": "Account does not exist",
  "status": "X02"
}
```

❌ Error : This error occurs if the virtual account number doesn't exist.

✅ Fix : Make sure your virtual account number is correct

---

```json
{
  "response_code": 500,
  "response_message": "Could not generate virtual account at the moment, please try again.",
  "status": "X02"
}
```

❌ Error : This error occurs due to a network error

✅ Fix : Please try again or contact the Payaza Support team.

---


### Payment Links


```json 
{
  "response_code": 500,
  "response_message": "Something went wrong. Please try again later."
}
```

❌ Error : This error occurs when the Fee_Bearer field is empty

✅ Fix : Make sure the Fee_bearer field isn't empty

---

```json                
{
  "response_code": 500,
  "response_message": "Payment Link not found!"
}
```

❌ Error : This error occurs when the payment link id is incorrect or doesn't exist.

✅ Fix : Make sure the payment link id value is correct.

---

```json
{
  "response_code": 500,
  "response_message": "Payment Link URL is not unique!"
}
```

❌ Error : This error occurs when the link parameter is empty or the link that is being placed isn't unique to the user.
                
✅Fix : Make sure the link parameter isn't empty or the value is unique to the Payment Link

---

```json                
{
  "response_code": 501,
  "response_message": "unable to parse request content json."
}
```   

❌ Error : This error occurs when different values are used compared to what is in the documentation.

✅ Fix : Make sure that the values of the payment_link_status parameter are either Active Or Deactivated

---


### Virtual Gift Card

```json
{
  "status": "error",
  "message": "Card Record not found [Invalid Card Reference]",
  "card_reference": "TRD122023061 "
}
```

❌ Error : This error occurs when an invalid card reference is used

✅ Fix : Put a valid card reference and retry

---

```json
{
  "status": "error",
  "message": "Duplicate Card Reference",
  "card_reference": "DOVJDAO1234e56"
}
```

❌ Error : This error occurs when a duplicate card reference is used

✅ Fix : Put a new card reference and retry

---

```json                
{
  "status": "error",
  "message": "One or more required parameters missing [currency,card_reference,first_name,last_name,email_address,home_address,phone_number]",
  "card_reference": "DOVJDAO1234e5"
}
```               

❌ Error : This occurs when certain parameters are missing or are not inputted properly


✅ Fix : Pass the required parameter(s) and retry

---

```json
{
  "status": "error",
  "message": "Card Record not found [Invalid Card Reference]",
  "transactions": []
}
```                

❌ Error : This error occurs when an invalid card reference is used

✅Fix : Put a valid card reference and retry

---

```json                
{
  "status": "error",
  "message": "Card Record not found [Invalid Card Reference]",
  "card_reference": "DOVE007"
}
```                

❌ Error : This error occurs when an invalid card reference is used

✅ Fix : Put a valid card reference and retry

--- 


### Fund Virtual Gift Card
                
```json
{
  "status": "error",
  "message": "You do not have enough balance to fund virtual gift card. Your available balance is: USD 10.94",
  "card_reference": "DOVJDAO1234e5"
}
```               

❌ Error : This error occurs when there is insufficient balance

✅ Fix : Put a valid card reference

---

```json                
{
  "status": "error",
  "message": "Card Record not found [Invalid Card Reference]",
  "card_reference": "DOVJDAO1234e"
}
```               

❌ Error : This error occurs when an invalid card reference is used

✅ Fix : Put a valid card reference and retry

---

```json
{
  "status": "error",
  "message": "Duplicate Transaction Reference",
  "card_reference": "DOVJDAO1234e5",
  "transaction_reference": "MDFGC85058204354"
}
```               

❌ Error : This error occurs when a duplicate transaction reference is used

✅ Fix : Put a new transaction reference and retry

---

```json                
{
  "status": "error",
  "message": "Funding transaction with reference MDFGC85058204353 failed",
  "card_reference": "DOVJDAO1234e5"
}
```

❌ Error : This error occurs when funding your virutal gift card

✅ Fix : Kindly retry again

---


### Card Acquiring Errors

```json
{
  "statusOk": false,
  "message": "Transaction Failed",
  "debugMessage": "Value 'TEST1213TEST' is invalid. There is already an authentication outcome associated with the supplied transaction ID.  To perform another authentication on the order, provide a new transaction ID.",
  "waitForNotification": false,
  "do3dsAuth": false,
  "paymentCompleted": false,
  "amountPaid": 0,
  "valueAmount": 0
}
```


❌ Error : This error occurs when there is a duplicate transaction reference

✅ Fix :Put a new transaction reference and retry

---

```json
{
  "statusOk": false,
  "message": "Transaction Failed",
  "debugMessage": "Value '401200xxxxxx1111' is invalid. Unable to determine card payment.",
  "waitForNotification": false,
  "do3dsAuth": false,
  "paymentCompleted": false,
  "amountPaid": 0,
  "valueAmount": 0
}
``` 

❌Error : This error occurs when wrong card details are used to make a transaction

✅ Fix : Enter the correct details

---

```json                
{
  "statusOk": false,
  "message": "Transaction Failed",
  "debugMessage": "Expired card",
  "description": "Test",
  "waitForNotification": false,
  "do3dsAuth": false,
  "paymentCompleted": false,
  "amountPaid": 0,
  "valueAmount": 0
}
```

❌ Error : Issue: This error occurs when an expired card is used.

✅Fix : Use a new card

---

### Checkout

```json
{
  "type": "error",
  "status": 400,
  "data": {
    "message": "Error during validation",
    "errors": [
      {
        "field": "merchant_key",
        "errors": [
          "'merchant_key' is required"
        ]
      },
      {
        "field": "checkout_amount",
        "errors": [
          "'checkout_amount' must be numeric"
        ]
      },
      {
        "field": "first_name",
        "errors": [
          "'first_name' cannot be blank"
        ]
      },
      {
        "field": "email_address",
        "errors": [
          "'email_address' cannot be blank",
          "'email_address' must be a valid email address"
        ]
      }
    ]
  }
}
```

❌ Error : This error occurs when a value is not provided or does not match what is described in the documentation

✅ Fix : Refer to our parameters to find the accepted formats and required fields, then proceed to include them before retrying the transaction.

---

### Add Beneficiary To Payaza Account

```json
{
  "response_code": 500,
  "response_message": "bank_code cannot be null or empty"
}
```

❌ Error : This error occurs if a bank_code parameter value is not 6 digits or is empty

✅ Fix : Ensure that the bank code is a 6-digit number

---

### Account Name Enquiry

```json
{
  "response_code": 500,
  "response_message": "Bank code should be 6 values"
}
```

❌ Error : This occurs when the bank code value is not 6 values

✅ Fix : Make sure the bank code is in an integer format and has a 6-digit length.

---

```json 
{
  "response_code": 500,
  "response_message": "Unknown Bank Code"
}
```                

❌ Error : This error occurs when the bank code is incorrect

✅ Fix : Put a valid bank code and retry

---

```json
{
  "response_code": 500,
  "response_message": "Invalid Account"
}
```                

❌ Error : This error occurs when the account number isn't correct.

✅ Fix : Pass a valid account number

---

```json
{
  "response_code": 500,
  "response_message": "Account number should be 10 values"
}
```                

❌ Error : This error occurs when the account number is not 10 digits. Note: This is for NGN payouts only.         

✅ Fix : Make sure that the account number is a 10-digit NUBAN number.

---

### Instant Collection And Payout

```json
{
  "response_code": 201,
  "response_message": "Payment amount cannot be less than payout amount"
}
```               

❌ Error : The payment (collection) amount is less than the payout amount

✅ Fix : Refer to the parameters, and reduce the amount accordingly before retrying

--- 

# Payment Page

Welcome to the Payment Page documentation. This guide will help you integrate our payment gateway and implement a secure and seamless checkout experience on your website.

### Introduction
Payment Page provides a secure and PCI-compliant solution for processing online payments. It allows your customers to complete their transactions on a secure payment page hosted by our payment gateway, reducing your PCI scope and ensuring a seamless checkout experience.

### Getting started

#### Retrieving your API Keys

To use our Payment Page Checkout, you will need an API key. Your API keys are available on your dashboard. Follow the steps below to access them:

- Login to your Payaza dashboard

- Click on settings in the navigation

- Click on API Key and Webhooks on the tab

#### Integration Steps
- Payment Page Checkout URL - https://business.payaza.africa/payment-page Redirects the customer to the Payment Page Checkout page, appending the following parameters to the URL provided above.

- On the Payment Page Checkout page, the customer will enter their payment details and complete the transaction

- Sample URL:
https://business.payaza.africa/payment-page/?merchant_key=PZ78-PKTEST-9A4086C1-UEHSHS9R&connection_mode=Test&checkout_amount=20¤cy_code=NGN&email_address=rayphil@gmail.com
&first_name=Ray&last_name=Phil&phone_number=08012345678&transaction_reference=b343aseasd
&additional_details={"user_id": 1273,"ticket": "TEUBD9382892"}&redirect_url=https://www.google.com.

```html
<!DOCTYPE html>
<html>
<head>
  <title>Sample Webpage</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      background-color: #f5f5f5;
      margin: 0;
      padding: 0;
    }
    
    .container {
      max-width: 800px;
      margin: 0 auto;
      padding: 20px;
      background-color: #ffffff;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }
    
    h1 {
      color: #333333;
      text-align: center;
    }
    
    form {
      max-width: 400px;
      margin: 20px auto;
    }
    
    label {
      display: block;
      font-weight: bold;
      margin-bottom: 5px;
    }
    
    input[type="text"],
    input[type="number"],
    input[type="email"],
    input[type="tel"] {
      width: 100%;
      padding: 10px;
      margin-bottom: 10px;
      border: 1px solid #dddddd;
      border-radius: 3px;
    }
    
    select {
      width: 100%;
      padding: 10px;
      margin-bottom: 10px;
      border: 1px solid #dddddd;
      border-radius: 3px;
    }
    
    input[type="submit"] {
      width: 100%;
      padding: 10px;
      background-color: #4caf50;
      color: #ffffff;
      border: none;
      border-radius: 3px;
      cursor: pointer;
    }
    
    input[type="submit"]:hover {
      background-color: #45a049;
    }
  </style>
</head>
<body>
  <div class="container">
    <h1>Sample Webpage</h1>
    
    <form action="process_form.php" method="POST" id="myForm">
      <label for="merchant_key">Merchant Key:</label>
      <input type="text" id="merchant_key" name="merchant_key" required>

      <label for="connection_mode">Connection Mode:</label>
      <select id="connection_mode" name="connection_mode">
        <option value="Live">Live</option>
        <option value="Test">Test</option>
      </select>

      <label for="checkout_amount">Amount:</label>
      <input type="number" id="checkout_amount" name="checkout_amount" required>

      <label for="currency_code">Currency Code:</label>
      <select id="currency_code" name="currency_code">
        <option value="NGN">NGN</option>
        <option value="USD">USD</option>
      </select>
      
      <label for="email_address">Email:</label>
      <input type="email" id="email_address" name="email_address" required>
      
      <label for="first_name">First Name:</label>
      <input type="text" id="first_name" name="first_name" required>

      <label for="last_name">Last Name:</label>
      <input type="text" id="last_name" name="last_name" required>

      <label for="phone_number">Phone:</label>
      <input type="tel" id="phone_number" name="phone" required>
      
      
      <label for="transaction_reference">Reference:</label>
      <input type="text" id="transaction_reference" name="transaction_reference" required>

      <!-- These are to be sent in the addition_details in json format. They are specific to your use case if you have any extra parameter you want to see on your dashboard -->
      <!-- This is an example for an Airline -->
     
      <label for="pnr">PNR:</label>
      <input type="text" id="pnr" name="Passenger Name Record" required>

      <label for="ticket_number">Ticket Number:</label>
      <input type="text" id="ticket_number" name="Ticket Number" required>

                 
      <label for="redirect_url">Redirect URL:</label>
      <input type="text" id="redirect_url" name="redirect_url" required>

      <input type="submit" value="Submit">
    </form>

  <script>
    document.getElementById("myForm").addEventListener("submit", function(event) {
      event.preventDefault(); // Prevent form submission
      
      // Get form field values
      var merchant_key= document.getElementById("merchant_key").value;
      var connection_mode= document.getElementById("connection_mode").value;
      var checkout_amount = document.getElementById("checkout_amount").value;
      var currency_code = document.getElementById("currency_code").value;
      var email_address = document.getElementById("email_address").value;
      var first_name = document.getElementById("first_name").value;
      var last_name = document.getElementById("last_name").value;
      var phone_number = document.getElementById("phone_number").value;
      var transaction_reference = document.getElementById("transaction_reference").value;

      // Note that these are to be passed as optional parameters
      var pnr = document.getElementById("pnr").value;
      var ticket_number = document.getElementById("ticket_number").value;
      
      // compose the additional details as a JSON object
      var additional_details = JSON.stringify({
        "pnr": pnr,
        "ticket_number" : ticket_number
        });

      var redirect_url = document.getElementById("redirect_url").value;
      
      // Build URL with form field values
      var url = "https://business.payaza.africa/payment-page?merchant_key=" + encodeURIComponent(merchant_key) +
                "&connection_mode=" + encodeURIComponent(connection_mode) +
                "&checkout_amount=" + encodeURIComponent(checkout_amount) +
                "&currency_code=" + encodeURIComponent(currency_code) +
                "&email_address=" + encodeURIComponent(email_address) +
                "&first_name=" + encodeURIComponent(first_name) +
                "&last_name=" + encodeURIComponent(last_name) +
                "&phone_number=" + encodeURIComponent(phone_number) +
                "&transaction_reference=" + encodeURIComponent(transaction_reference) + 
                "&additional_details=" + encodeURIComponent(additional_details) + // Encoded JSON; Note that this is optional 
                "&redirect_url=" + encodeURIComponent(redirect_url);
        const a = document.createElement('a')
        a.href = url
        a.click()
      
      // Redirect to the constructed URL
      // window.location.href = "http://127.0.0.1:5500/Webpage.html";
    });
  </script>

  </div>
</body>
</html>
```

### Arguments

merchant_keystring

Your Public API key

---

connection_mode string

Connection mode. (Live or Test)

---

checkout_amount double

Amount to charge the customer

---

currency_code string

Currency to charge in.

---

email_address string

The email address of the customer

---

first_name string

The first name of the customer

---

last_name string

The last name of the customer

---

phone_number int

The phone number of the customer

---

transaction_reference string

The unique identifier given to a particular transaction by the merchant.

---

additional_details JSON

Custom data to your payload (should be encoded)

---

redirect_url string

The URL the customer is redirected to (should be encoded). This must be the last parameter in the request if included

---

### Payment Options

Our Payment Page supports a wide range of payment options, including debit cards, and alternative payment methods.

#### Security

We take security seriously and ensure that our Payment Page is compliant with the latest PCI-DSS standards. Our payment gateway encrypts sensitive data and employs robust security measures to protect your customers' payment information.


# Transfers
### How does it work?

Transfers are transactions made from your Payaza account to beneficiaries. Merchants can instantly transfer funds from their available balance. For every transfer, you need to specify the amount and the beneficiary’s details. You become eligible to make transfers after you have been verified. (you sign up, finish account activation and KYC verification)

- Fund your Payaza account, this can be done directly or through settlement.
- You must have created a transaction PIN.

| **Transfer APIs**  |
|:--------------|
| [Initiate A Transfer](https://docs.payaza.africa/developers/apis/make-payments/transfers/initiate-transfer)
This endpoint initiates a transfer with your Payaza account.|
[Transaction Status Query](https://docs.payaza.africa/developers/apis/make-payments/transfers/transaction-status-query)
This endpoint gets the details of a particular transaction by the reference  |
| [Get Account Name Enquiry](https://docs.payaza.africa/developers/apis/make-payments/transfers/account-name-enquiry)
This endpoint fetches the account details of an account number | 
[Bank Codes](https://docs.payaza.africa/developers/apis/make-payments/transfers/bank-codes)
This endpoint fetches the bank codes available for each country|

# Initiate a Transfer
## Introduction

This endpoint initiates a transfer from your Payaza account to a bank account, and also to a mobile money account.


    Note

    Payaza bank codes can be accessed here

    Please be advised that payouts to countries other than Nigeria are exclusively available upon request. To initiate this process, kindly send an email to support@payaza.africa. You will be granted access once our team reviews and approves your request.

    The account_reference value is retrieved when you use the View Payaza Account Details API here

### Arguments
transaction_typestring

The type of account being transferred to.(Default is NGN)
**NGN** == “nuban”
**GHS** == “mobile_money” or “ghipps”
**UGX** == “mobile_money”
**TZS** == “mobile_money” or “tiss”
**KES** == “mobile_money” or “kepss”
**XOF** == “mobile_money” or “wave”
**XAF** == “mobile_money”
**ZAR** == “RTC”

---

service_payload object

It contains the details of the service payload.

---

payout_amount double

The amount of the transaction

---

transaction_pin int

This is the merchant's unique transaction pin

---

account_reference string

The reference of the Payaza account. Please be advised that your Payaza Account possesses unique account references corresponding to each currency utilized.(This is available in the “View Payaza Account Details” API response body)

---

currency string

The transfer currency code.e.g. NGN, GHS, TZS, UGX, KES, XAF, ZAR

---

country string Optional

The ISO 3166-1 alpha-3 country code e.g. BEN,CIV,CMR. This is required for XOF payouts

---

payout_beneficiaries array

Contains the details of the payout beneficiaries

---

credit_amount double

The amount to be credited to this particular beneficiary

---

account_number string

This is the account number of the beneficiary
For “nuban” transfer == 10 digits
For “kepss” transfer == 10 digits
For “mobile_money” == 12 digits for GHA, TZA, UGA, KEN, CMR and 13 digits for CIV, BEN (starting with the 3-digit country code) e,g.
Ghana = 233xxxxxxxxx,
Kenya = 254xxxxxxxxx
Uganda = 256xxxxxxxxx
Tanzania = 255xxxxxxxxx
Cameroon = 237xxxxxxxxx
Benin = 229(01)xxxxxxxx
Cote D'ivoire = 225xxxxxxxxxx, etc.

---

account_name string

This is the name of the beneficiary for the transfer

---

bank_code string

This is the beneficiary's bank code

---

narration string

Narration for the payout (This must be 25 characters or less with no special characters)

---

transaction_reference string

This is the unique identifier generated for each transaction by the merchant

---

sender object

Contains the details of the sender

---

sender_name string

The name of the sender

---

sender_id string Optional

The unique identifier of the sender

---

sender_phone_number string

Phone number of the sender

---

sender_address string

The address of the sender

---


### Key Parameters
The provided API request body is designed to accommodate transactions across various currencies. However, there are four parameters that vary depending on the country:

- **transaction_type**: This parameter signifies the category or type of account receiving the transfer.
- **account_reference**: This parameter represents the unique reference associated with the Payaza account.
- **currency**: This parameter represents the currency code for the transfer, which varies based on the country involved such as NGN, GHS, TZS, UGX, KES and XOF.
- **country**: This parameter represents the ISO 3166-1 alpha-3 country code. This is required for XOF payments

### Detailed Description of Parameters

- **transaction_type**:

Definition: The transaction_type parameter signifies the category or type of account that is the recipient of the transfer, which varies by country.

Usage: Depending on the country-specific requirements, the transaction_type value should be set accordingly to facilitate accurate transfers, as different countries may have distinct account classifications.


| **Currencies** | **“transaction_type” Value**     | **Bank Transfer	Mobile Money** |
|:--------------:|:-------------------:|:--------------------:|
| **NGN (Nigeria)**       | nuban    |                  |
| **GHS (Ghana)** | ghipps    | mobile_money                 |
| **KES (Kenya)** | kepss    | mobile_money                |
| **TZS (Tanzania)** | tiss    | mobile_money        |
| **UGX (Uganda)** |         | mobile_money                 |
| **XOF(Benin)** |           | mobile_money                |
| **XOF(Cote D'Ivoire)** |      | mobile_money                |
| **XOF(Cote D'Ivoire)** |     | wave        |
| **XAF(Cameroon)** |         | mobile_money                 |
| **ZAR(South Africa)** |     RTC      |                 |


- **account_reference**:

Definition: The account_reference parameter indicates the unique reference of the Payaza account corresponding to each currency utilized.

Usage: Each currency has a distinct account reference attached to it, retrieved from the View Payaza Account Details API, ensuring precise transactions based on the designated account.


    Note

    Kindly note that the account_reference is payazaAccountReference in the View Payaza Account Details Response body

Below is a Truncated View Payaza Account Details Sample Response

```json
 {
    "message": "Account enquiry response",
    "status": true,
    "data": [
        {
            "id": 3526,
            "accountName": "Integrations",
            "payazaAccountReference": "5012345678",
            "status": "ACTIVE",
            "accountBalance": 14300.00,
}]
```

- **currency**:

Definition: The currency parameter specifies the transfer currency code (e.g., NGN, GHS) for the transaction.

Usage: The currency code must align with the desired currency for the transfer.


    Note

    Kindly note that the Default is NGN

- **country**:

Definition: The country parameter represents the ISO 3166-1 alpha-3 country code.

Usage: Use the desired country code for your transfer.


    Note

    Kindly note that this is required for XOF payments.


## Authorization Header Values
Authorization string

Payaza Base 64 encoded merchant's API key

---

x-TenantID string

Connection mode: “live” or “test”

---

```json
{
"authorization":"Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC",
"x-TenantID":"test"
}
```

```java
Initiate A Transfer

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "transaction_type": "nuban",
    "service_payload": {
        "payout_amount": 100,
        "transaction_pin": 1111,
        "account_reference": "1010000009",
        "currency": "NGN",
        "country": "NGA",
        "payout_beneficiaries": [
            {
                "credit_amount": 100,
                "account_number": "9207067319",
                "account_name": "John Doe",
                "bank_code": "000013",
                "narration": "Test",
                "transaction_reference": "TD93001234",
                "sender": {
                    "sender_name": "Jane Doe",
                    "sender_id": "",
                    "sender_phone_number": "01234595",
                    "sender_address": "123, Ace Street"
                }
            }
        ]
    }
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payout-receptor/payout")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS1RFU1QtRUI0RjQ5NTEtRDhENy00RjFCLUI5REItMjdBQTc5RDU1MzE2")
  .addHeader("X-tenantID", "test")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

```json
RESPONSE
{
  "response_code": 200,
  "response_message": "Request successfully submitted",
  "response_content": {
    "transaction_status": "09",
    "narration": "Payout",
    "transaction_time": "2023-10-19T14:37:35.517809",
    "amount": 100,
    "response_status": "TRANSACTION_INITIATED",
    "response_description": "Transaction has been successfully submitted for processing"
  },
  "resp_code": "09"
}
```

```json
GHS Bank Transfer

 "transaction_type": "ghipps"
"service_payload": {
  "account_reference": "3012345678",
  "currency": "GHS",
  "country": "GHA"
  ...
```

```json
XOF (Benin) mobile money

  "transaction_type": "mobile_money"
"service_payload": {
  "account_reference": "9012345678",
  "currency": "XOF"
  "country": "BEN",
  ...
```

---

# Get Account Name Enquiry
## Introduction

This endpoint fetches the account details.


    Note

    Copy the key from your dashboard, encode it to base 64 and paste in the Authentication header with the “Payaza” Prefix

## Arguments

---

service_payload object

It contains the details of the service payload.

---

currency string

The currency code

---

bank_code string

The bank code

---

account_number string

This is the account number

---

POST Endpoint

```
POST: https://api.payaza.africa/live/payaza-account/api/v1/mainaccounts/merchant/provider/enquiry
```

```java
Get Account Name Enquiry

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "service_payload": {
        "currency":"NGN", 
        "bank_code": "090123",
        "account_number": "0103937899"
    }
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/mainaccounts/merchant/provider/enquiry")
  .method("POST", body)
  .addHeader("X-TenantID", "test")
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtNUYyMzk1RTAtMUVEMy00MjJCLUIzOEMtMEYyNzg5")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute(); 
```

```json

RESPONSE

{
  "response_code": 200,
  "response_message": "Request successfully submitted",
  "response_content": {
    "transaction_status": "09",
    "narration": "Payout",
    "transaction_time": "2023-10-19T14:37:35.517809",
    "amount": 100,
    "response_status": "TRANSACTION_INITIATED",
    "response_description": "Transaction has been successfully submitted for processing"
  },
  "resp_code": "09"
} 

```

### Authorization Header Values

---

Authorization string

Payaza Base 64 encoded merchant's API key

---

x-TenantID string

Connection mode: “live” or “test”

---

```json
{
"authorization":"Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC",
"x-TenantID":"test"
}
```

---

# Transaction Status Query
## Introduction

This endpoint gets the details of a particular transaction by the reference.

## Transaction Statuses

Find below a representation of possible transaction statuses you can get as a response and their meanings

| **Status** | **MEANING**  |
|:--------------:|:-------------------:|
| TRANSACTION_INITIATED       | The transaction has been received and queued for processing    |
| NIP_SUCCESS | The transaction is successful.      |
| NIP_PENDING| 	The transaction is still in progress.    |
| NIP_FAILURE      | The transaction has failed.    |
| ESCROW_SUCCESS | The amount has been deducted, but it is being processed by the bank, and it can be reversed due to network problems.      |


Get endpoint

```
GET: https://api.payaza.africa/live/payaza-account/api/v1/mainaccounts/merchant/transaction/{{transaction_reference}} 
```

```java
Transaction Status Query


OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/mainaccounts/merchant/transaction/{{transaction_reference}}")
  .method("GET", body)
  .addHeader("X-TenantID", "test")
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRDhGNzY0NjctMEMwQS00M0M3LUEwNTUtNDRCMkVFN0M2OTUy")
  .build();
Response response = client.newCall(request).execute();     
```

```json
RESPONSE

{
  "response_code": 200,
  "response_message": "Request successfully submitted",
  "response_content": {
    "transaction_status": "09",
    "narration": "Payout",
    "transaction_time": "2023-10-19T14:37:35.517809",
    "amount": 100,
    "response_status": "TRANSACTION_INITIATED",
    "response_description": "Transaction has been successfully submitted for processing"
  },
  "resp_code": "09"
} 
```


### Arguments

transaction_reference string

The unique identifier given to a particular transaction

---

### Authorization Header Values
---

Authorization string

Payaza Base 64 encoded merchant's API key

---

x-TenantID string

Connection mode: “live” or “test”

---

```json
GET
          
{ 
 "authorization": Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC,
 "x-TenantID": test
}
```


# Bank Codes
## Introduction
This endpoint fetches the bank codes available for each country


    Note

    Copy the key from your dashboard, encode it to base 64 and paste in the Authentication header with the “Payaza” Prefix

### Arguments

---

currency_code string

The currency code of the country

---

Get endpoint

```json
GET: https://api.payaza.africa/live/payaza-account/api/v1/mainaccounts/merchant/banks/{{currency_Code}}
```

```java
Bank Codes

  OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/mainaccounts/merchant/banks/{{currency_code}}")
  .method("GET", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS1RFU1QtRUI0RjQ5NTEtRDhENy00RjFCLUI5REItMjdBQTc5RDU1MzE2")
  .addHeader("x-tenantID", "live")
  .build();
Response response = client.newCall(request).execute();
         
```

```json
RESPONSE

{
  "response_code": 200,
  "response_message": "Request successfully submitted",
  "response_content": {
    "transaction_status": "09",
    "narration": "Payout",
    "transaction_time": "2023-10-19T14:37:35.517809",
    "amount": 100,
    "response_status": "TRANSACTION_INITIATED",
    "response_description": "Transaction has been successfully submitted for processing"
  },
  "resp_code": "09"
} 

```


### Authorization Header Values

---

Authorization string

Payaza Base 64 encoded merchant's API key

---

x-TenantID string

Connection mode: “live” or “test”

---

```json
POST
          
{ 
 "authorization": Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC,
 "x-TenantID": test
}
```

# Payaza Account

## Payaza Account

### How does it work?

This is our internal account that can be used to make payouts to other bank accounts, in single or in bulk. The Payaza account can also be used to receive settlements.

- Track the transfer status of your transactions
- Check your available balance instantly


| **Payaza Account APIs**  |
|:--------------|
| [View Payaza Account details](https://docs.payaza.africa/developers/apis/make-payments/payaza-account/payaza-account-details)
Retrieves Payaza account details.|


## Introduction

This endpoint returns the Payaza account details for a merchant.

GET Endpoint

```
GET: https://api.payaza.africa/live/payaza-account/api/v1/mainaccounts/merchant/enquiry/main
```

```java
View Payaza Account Details

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/mainaccounts/merchant/enquiry/main")
  .method("GET", body)
  .addHeader("X-TenantID", "test")
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtQkNDRUIwMEMtODdBNC00RjJBLUEyNUItNDM5MzE")
  .build();
Response response = client.newCall(request).execute();
 
```

```json
RESPONSE

{
  "message": "Account enquiry response",
  "status": true,
  "data": [
    {
      "id": 1,
      "accountName": "Test Merchant",
      "payazaAccountReference": "1010000000",
      "status": "ACTIVE",
      "accountBalance": 990.13,
      "businessId": 92,
      "currency": "NGN",
      "country": "NGA",
      "organizationName": "PAYAZA",
      "productCode": "PAYOUT-MAIN-NGN",
      "productNumber": "101",
      "postNoCredit": false,
      "postNoDebit": false,
      "originatorName": null,
      "pauseTransactions": null,
      "hasVirtualAccounts": true,
      "holdTransactionAtLowBalance": false,
      "virtualAccounts": [
        {
          "accountNumber": "99926838326",
          "accountName": "PAYAZA(Test Merchant)",
          "bankCode": "000023",
          "bankId": 306
        }
      ]
    },
    {
      "id": 1658,
      "accountName": "Test Merchant",
      "payazaAccountReference": "3010000000",
      "status": "ACTIVE",
      "accountBalance": 673.26,
      "businessId": 1,
      "currency": "GHS",
      "country": "NGA",
      "organizationName": "PAYAZA",
      "productCode": "PAYOUT-MAIN-GHS",
      "productNumber": "301",
      "postNoCredit": false,
      "postNoDebit": false,
      "originatorName": null,
      "pauseTransactions": null,
      "hasVirtualAccounts": false,
      "holdTransactionAtLowBalance": false,
      "virtualAccounts": []
    }
  ]
} 
```

### Authorization Header Values

---

Authorization string

Payaza Base 64 encoded merchant's API key

---

x-TenantID string

Connection mode: “live” or “test”

---

```json
GET
          
{ 
 "authorization": Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC,
 "x-TenantID": test
}
```


# EUR Transfers

Transfers are transactions made from your Payaza account to beneficiaries. Merchants can instantly transfer funds from their available balance. For every transfer, you need to specify the amount and the beneficiary’s details.

You become eligible to make transfers after you have been verified (i.e., you sign up, finish account activation, and complete KYC verification).

---

## Requirements

Before making EUR transfers:

- ✅ Fund your Payaza account — either directly or through settlement.  
- ✅ Create a transaction PIN.

---

## EUR Transfer APIs

### 1. Request Corporate Account
This endpoint allows merchants to request a corporate sub-account.

### 2. Request User Account
This endpoint allows merchants to request a user sub-account.

### 3. Get All Euro Account Request
This endpoint fetches all Euro account requests.

### 4. Get Single Euro Account Request
This endpoint retrieves the details of a particular transaction by the creation reference.

### 5. Initiate EUR Transfer
This endpoint initiates a EUR transfer.

### 6. List of Acceptable IDs
This endpoint shows the list of acceptable identification documents in various countries.

### 7. Fetch All Subaccounts
This endpoint displays all EUR subaccounts that have been created.

### 8. Fetch Euro Wallet
This endpoint retrieves the Euro wallet using its subaccount reference.


# Request Corporate Account

## Introduction
This endpoint allows merchants to request a **corporate sub-account**.

---

## Arguments

| Field | Type | Required | Description |
|-------|------|-----------|-------------|
| `country` | string | ✅ | The country ISO code (e.g., `US`) |
| `currency` | string | ✅ | The currency code (e.g., `EUR`) |
| `company_name` | string | ✅ | The registered company name |
| `purpose` | string | ✅ | The reason for opening the account |
| `certificate_of_incorporation` | string | ✅ | URL link to the certificate of incorporation |
| `mermat` | string | Optional | URL link to the company memorandum and articles of association |
| `consent` | boolean | ✅ | Whether the company consents to the account request |
| `directors` | array | ✅ | List of directors with identification and proof of address |
| `shareholders` | array | Optional | List of shareholders with ID and proof of address |
| `category` | string | ✅ | Account category (e.g., `SUB_ACCOUNT`) |
| `account_type` | string | ✅ | Type of account (e.g., `corporate`) |
| `main_account_payaza_reference` | string | ✅ | The main Payaza account reference number |

---

## POST Endpoint

**URL:**  

POST: https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merc

```java
Request Corporate Account


OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "country": "NG",
    "purpose": "test purposes",
    "currency":"EUR",
    "category":"SUB_ACCOUNT",
    "certificate_of_incorporation": "https://payaza-assets.s3.amazonaws.com/development/1737370134640-number2.png",
    "mermat": "https://payaza-assets.s3.amazonaws.com/development/1737370128708-number1.jpg",
    "consent": true,
    "account_type": "corporate",
    "main_account_payaza_reference": "10987654312",
    "company_name": "Steve Stones Plc",
    "directors": [
        {
            "id_file": "https://payaza-assets.s3.amazonaws.com/development/1737370156723-number3.jpg",
            "id_type": "passport",
            "poa_file": "https://payaza-assets.s3.amazonaws.com/development/1737370165810-number4.jpg",
            "poa_type": "utilityBill"
        },
        {
            "id_file": "https://payaza-assets.s3.amazonaws.com/development/1737370156723-number3.jpg",
            "id_type": "passport",
            "poa_file": "https://payaza-assets.s3.amazonaws.com/development/1737370165810-number4.jpg",
            "poa_type": "utilityBill"
        }
    ],
    "shareholders": [
        {
            "id_file": "https://payaza-assets.s3.amazonaws.com/development/1737370178824-number5.jpg",
            "id_type": "passport",
            "poa_file": "https://payaza-assets.s3.amazonaws.com/development/1737370187160-number6.png",
            "poa_type": "utilityBill"
        },
        {
            "id_file": "https://payaza-assets.s3.amazonaws.com/development/1737370178824-number5.jpg",
            "id_type": "passport",
            "poa_file": "https://payaza-assets.s3.amazonaws.com/development/1737370187160-number6.png",
            "poa_type": "utilityBill"
        },
        {
            "id_file": "https://payaza-assets.s3.amazonaws.com/development/1737370178824-number5.jpg",
            "id_type": "passport",
            "poa_file": "https://payaza-assets.s3.amazonaws.com/development/1737370187160-number6.png",
            "poa_type": "utilityBill"
        }
    ]
}
");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/euro/request")
  .method("POST", body)
  .addHeader("authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .addHeader("x-api-key", "test")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();

```

### Responses

#### API Response

```json
{
  "message": "EUR Account request has been submitted for review.",
  "status": true,
  "creationReference": "ac456ecb-7870-47d4-af23-2dde8563235f"
}
```

#### Webhook Notification

```json
{
  "id": 80787,
  "transactionReference": "2ec90bf8-cf05-4082-a57e-100171e0f9ca",
  "notificationChannel": "Euro",
  "recipients": "https://webhook.site/dbb3297a-fbcf-4972-b229-8dab2c5d2738",
  "notification": {
    "reference": "2ec90bf8-cf05-4082-a57e-100171e0f9ca",
    "name": "Debs and Co",
    "accountType": "corporate",
    "currency": "EUR",
    "country": "DEU",
    "bic": "ARPYGB21XXX",
    "bankName": "Prune Payments LTD",
    "address": "Office 7 35-37 Ludgate Hill, London",
    "bankCountry": "United Kingdom",
    "category": "SUB_ACCOUNT",
    "iban": "GB12045505050505408"
  },
  "status": "PROVIDER_CREATED",
  "statusDescription": null
}
```

## Authorization Header Values

```json
POST
          
{ 
 "authorization": Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC,
 "x-TenantID": test
}
```

# Request User Account

## Introduction
This endpoint allows users to **request a personal (user) sub-account**.

---

> ⚠️ **Note:**  
> Copy the key from your dashboard, encode it to **Base64**, and include it in the **Authorization header** with the `Payaza` prefix.

---

## Arguments

| Field | Type | Required | Description |
|-------|------|-----------|-------------|
| `country` | string | ✅ | The country ISO code (e.g., `US`) |
| `currency` | string | ✅ | The currency code (e.g., `EUR`) |
| `first_name` | string | ✅ | User's first name |
| `last_name` | string | ✅ | User's last name |
| `purpose` | string | ✅ | Purpose of the account |
| `id_file` | string | ✅ | URL to the user's ID file |
| `id_file_back` | string | Optional | Required if `id_type` is not `passport` |
| `id_type` | string | ✅ | User’s ID type (`passport` or `identityCard`) |
| `poa_file` | string | ✅ | URL to proof of address document |
| `poa_type` | string | Optional | Type of proof of address (e.g., `utilityBill`) |
| `category` | string | ✅ | Account category (e.g., `SUB_ACCOUNT`) |
| `account_type` | string | ✅ | Type of account (`user`) |
| `main_account_payaza_reference` | string | ✅ | Main Payaza account reference |

---

## POST Endpoint

```
**URL:** POST https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/euro/request
```

```java
Request User Account

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "country": "US",
    "currency":"EUR",
    "category":"SUB_ACCOUNT",
    "purpose": "Sit consequuntur co",
    "consent": "true",
    "account_type": "user",
    "main_account_payaza_reference": "10987654312",
    "id_file": "https://payaza-assets.s3.amazonaws.com/development/1737369951133-number1.jpg",
    "id_type": "passport",
    "poa_file": "https://payaza-assets.s3.amazonaws.com/development/1737369961250-number2.png",
    "poa_type": "utilityBill",
    "first_name": "Steve",
    "last_name": "Stones"
}
");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/euro/request")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .addHeader("X-TenantID", "test")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
    
```

### Authorization Header Values

---

Authorization string

Payaza Base 64 encoded merchant's API key

---

x-TenantID string

Connection mode: “live” or “test”

---

```json
{ 
 "authorization": Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC,
 "x-TenantID": test
}
```

# 🏦 Get All Euro Account Request

## **Introduction**
This endpoint fetches all the Euro account requests associated with your Payaza merchant account.

---

## **Note**
Copy the API key from your dashboard, encode it in **Base64**, and include it in the **Authorization** header with the `"Payaza"` prefix.

---

## **Arguments**

| Name | Type | Required | Description |
|------|------|-----------|--------------|
| `page_size` | `int` | ✅ Yes | Specifies the number of results to be fetched per page. |

---

## **GET Endpoint**

```http
GET https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/euro/request
```

---

## **Example Request**

```java
OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/euro/request")
  .method("GET", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .addHeader("X-TenantID", "test")
  .build();
Response response = client.newCall(request).execute();
```

---

## **Response**

The API will return a JSON response containing details of all Euro account requests associated with your merchant profile.

---

## **Authorization Header Values**

| Header | Type | Description |
|---------|------|--------------|
| `Authorization` | `string` | `Payaza` followed by your **Base64 encoded merchant API key** |
| `X-TenantID` | `string` | Connection mode — `"live"` or `"test"` |

---

### **Example Header**

```json
{ 
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC",
  "x-TenantID": "test"
}
```

# 🇪🇺 Get Single Euro Account Request

## **Introduction**
This endpoint fetches a specific Euro account request using its **Creation Reference**.

---

## **Arguments**

| Name | Type | Required | Description |
|------|------|-----------|--------------|
| `creationReference` | `string` | ✅ Yes | The unique request creation reference identifier. |

---

## **GET Endpoint**

```http
GET https://api.payaza.africa/live/payaza-account/api/v1/euro/account/{{creationReference}}
```

---

## **Example Request**

```java
Get single Euro Account Request

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/euro/account/{{creationReference}}")
  .method("GET", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .addHeader("X-TenantID", "test")
  .build();
Response response = client.newCall(request).execute();
```

---

## **Response**

A successful response returns details of the Euro account request.

### **Example Response**

```json
{
  "id": 80787,
  "transactionReference": "2ec90bf8-cf05-4082-a57e-100171e0f9ca",
  "notificationChannel": "Euro",
  "recipients": "https://webhook.site/dbb3297a-fbcf-4972-b229-8dab2c5d2738",
  "notification": {
    "reference": "2ec90bf8-cf05-4082-a57e-100171e0f9ca",
    "name": "Debs and Co",
    "accountType": "corporate",
    "currency": "EUR",
    "country": "DEU",
    "bic": "ARPYGB21XXX",
    "bankName": "Prune Payments LTD",
    "address": "Office 7 35-37 Ludgate Hill, London",
    "bankCountry": "United Kingdom",
    "category": "SUB_ACCOUNT",
    "iban": "GB12045505050505408"
  },
  "status": "PROVIDER_CREATED",
  "statusDescription": null
}
```

---

## **Authorization Header Values**

| Header | Type | Description |
|---------|------|-------------|
| `Authorization` | `string` | `Payaza` followed by your Base64 encoded merchant API key |
| `X-TenantID` | `string` | Connection mode — `"live"` or `"test"` |

---

### **Example Header**

```json
{ 
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC",
  "x-TenantID": "test"
}
```

# 💶 Initiate EUR Transfer

## **Introduction**
This endpoint initiates a **EUR transfer** from your Payaza account to a EUR account.

---

> **Note:**  
> Copy the key from your dashboard, encode it to Base64, and include it in the `Authorization` header with the `Payaza` prefix.

---

## **Arguments**

| Name | Type | Required | Description |
|------|------|-----------|--------------|
| `transaction_type` | `string` | ✅ Yes | The type of account being transferred to. For EUR, use `"sepa"`. |
| `service_payload` | `object` | ✅ Yes | Contains details of the service payload. |
| `payout_amount` | `double` | ✅ Yes | The transaction amount. |
| `transaction_pin` | `int` | ✅ Yes | The merchant’s unique transaction PIN. |
| `account_reference` | `string` | ✅ Yes | The reference of the Payaza account. Each currency has a unique account reference (found in the “View Payaza Account Details” API response). |
| `currency` | `string` | ✅ Yes | The currency code. |
| `country` | `string` | ❌ Optional | The country code. |
| `payout_beneficiaries` | `array` | ✅ Yes | List of payout beneficiaries and their details. |
| `destination_country` | `string` | ✅ Yes | The beneficiary’s country code. |
| `bank_name` | `string` | ✅ Yes | The beneficiary’s bank name. |
| `credit_amount` | `double` | ✅ Yes | Amount to be credited to the beneficiary. |
| `account_number` | `string` | ✅ Yes | The beneficiary’s account number. |
| `account_name` | `string` | ✅ Yes | The beneficiary’s account name. |
| `bank_code` | `string` | ✅ Yes | The beneficiary’s bank code (SWIFT/BIC format: AAAABBCC123). |
| `narration` | `string` | ❌ Optional | Transaction narration (≤ 25 characters, no special characters). |
| `transaction_reference` | `string` | ✅ Yes | Unique identifier for the transaction (generated by the merchant). |
| `sender` | `object` | ✅ Yes | Contains sender details. |
| `sender_name` | `string` | ✅ Yes | Name of the sender. |
| `sender_id` | `string` | ❌ Optional | Unique identifier of the sender. |
| `sender_phone_number` | `string` | ✅ Yes | Phone number of the sender. |
| `sender_address` | `string` | ✅ Yes | Address of the sender. |

---

## **POST Endpoint**

```http
POST https://api.payaza.africa/live/payout-receptor/payout
```

---

## **Example Request (OkHttp)**

```java
OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    \"transaction_type\": \"sepa\",
    \"service_payload\": {
        \"payout_amount\": 300,
        \"transaction_pin\": 1987,
        \"account_reference\": \"88980086362\",
        \"country\": \"NGA\",
        \"currency\": \"EUR\",
        \"payout_beneficiaries\": [
            {
                \"destination_country\": \"NGA\",
                \"bank_name\": \"Prune\",
                \"credit_amount\": 300,
                \"account_number\": \"8185003687\",
                \"account_name\": \"John Doe\",
                \"bank_code\": \"ARPYGB21XXX\",
                \"narration\": \"Test\",
                \"transaction_reference\": \"TDd9frfk\",
                \"sender\": {
                    \"sender_name\": \"Jane Doe\",
                    \"sender_id\": \"32\",
                    \"sender_phone_number\": \"0124595\",
                    \"sender_address\": \"123, Ace Street\"
                }
            }
        ]
    }
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payout-receptor/payout")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS1RFU1QtQzdGNzdBNDItMTQyQy00NEFGLTlBM0ItRUEwQzUyN0VCRjVG")
  .addHeader("X-TenantID", "test")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

---

## **Response**

The API responds with the status of the EUR transfer request.

---

## **Authorization Header Values**

| Header | Type | Description |
|---------|------|-------------|
| `Authorization` | `string` | `Payaza` followed by your Base64 encoded merchant API key |
| `X-TenantID` | `string` | Connection mode — `"live"` or `"test"` |

---

### **Example Header**

```json
{ 
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC",
  "x-TenantID": "test"
}
```

# List of Acceptable IDs

## Introduction

This endpoint shows the list of acceptable identification documents in various countries.

> **Note:**  
> Copy the key from your dashboard, encode it to Base64, and paste it in the Authentication header with the “Payaza” prefix.

---

## GET Endpoint

**GET**

```
https://api.payaza.africa/live/payaza-account/api/v1/euro/acceptable-ids
```

---

## List Of Acceptable IDs

```java
List Of Acceptable IDs

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/euro/acceptable-ids")
  .method("GET", body)
  .addHeader("Authorization", "Payaza ")
  .addHeader("X-TenantID", "test")
  .build();
Response response = client.newCall(request).execute();
```

---

## RESPONSE

```json
{
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC,",
  "x-TenantID": "test"
}
```

---

## Authorization Header Values

| Key | Type | Description |
|------|------|-------------|
| **Authorization** | string | Payaza Base 64 encoded merchant's API key |
| **x-TenantID** | string | Connection mode: "live" or "test" |


# Fetch all subaccounts
# Introduction

This endpoint is used to display all EUR subaccounts that have been created.

> **Note:**  
> Copy the key from your dashboard, encode it to Base64, and paste it in the Authentication header with the “Payaza” prefix.

---

## Arguments

| Name | Type | Description |
|------|------|-------------|
| **pageSize** | int | The number of entries in a page. |
| **pageNumber** | int | The number of pages. |
| **currency** | string | The currency of the subaccount(s). This is **EUR**. |
| **searchPhrase** | string *(Optional)* | Used to search for the account name. |

---

## POST Endpoint

**POST**

```
https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/enquiry/business
```

---

## Fetch All Sub Accounts (Euro)

```java
Fetch All Sub Accounts (Euro)

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
  "pageSize": 10,
  "pageNumber": 1,
  "currency": "EUR",
  "searchPhrase": ""
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/enquiry/business")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .addHeader("X-TenantID", "test")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

---

## RESPONSE

```json
{
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC",
  "x-TenantID": "test"
}
```

---

## Authorization Header Values

| Key | Type | Description |
|------|------|-------------|
| **Authorization** | string | Payaza Base 64 encoded merchant's API key |
| **x-TenantID** | string | Connection mode: "live" or "test" |


# Fetch Euro Wallet
# Introduction

This endpoint is used to show the Euro wallet using its subaccount reference.

> **Note:**  
> Copy the key from your dashboard, encode it to Base64, and paste it in the Authentication header with the “Payaza” prefix.

---

## GET Endpoint

**GET**

```
https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/enquiry/{{sub-reference}}
```

---

## Fetch Euro Wallet

```java
OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/enquiry/{{sub-reference}}")
  .method("GET", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .addHeader("X-TenantID", "test")
  .build();
Response response = client.newCall(request).execute();
```

---

## RESPONSE

```json
{
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC",
  "x-TenantID": "test"
}
```

---

## Authorization Header Values

| Key | Type | Description |
|------|------|-------------|
| **Authorization** | string | Payaza Base 64 encoded merchant's API key |
| **x-TenantID** | string | Connection mode: "live" or "test" |


# Sub Accounts
# Create A Sub Account

# Introduction

This endpoint creates a sub account.

---

## Arguments

| Parameter | Type | Description |
|------------|------|-------------|
| **mainAccountPayazaReference** | string | The reference of the main Payaza account |
| **name** | string | The name of this subaccount |
| **currency** | string | The currency code |
| **country** | string | The country code in ISO 3166-1 alpha-3 format |

---

## POST Endpoint

**POST**

```
https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant
```

---

## Create a Sub Account

```java
OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    \"mainAccountPayazaReference\": \"100000000\",
    \"name\": \"Test Sub Account\",
    \"currency\": \"NGN\",
    \"country\": \"NGA\"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant")
  .method("POST", body)
  .addHeader("X-TenantID", "test")
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTBNzdC")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

---

## RESPONSE

```json
{
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC",
  "x-TenantID": "test"
}
```

---

## Authorization Header Values

| Key | Type | Description |
|------|------|-------------|
| **Authorization** | string | Payaza Base 64 encoded merchant's API key |
| **x-TenantID** | string | Connection mode: “live” or “test” |


# View Payaza sub account details 
# Introduction

This endpoint returns the Payaza subaccount details.

---

## Arguments

| Parameter | Type | Description |
|------------|------|-------------|
| **payazaSubAccountReference** | string | The reference of the Payaza sub account |

---

## GET Endpoint

**GET**

```
https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/enquiry/{{payazaSubAccountReference}}
```

---

```java
View Payaza Sub Account Details

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/payaza-account/api/v1/subaccounts/merchant/enquiry/{{payazaSubAccountReference}}")
  .method("GET", body)
  .addHeader("X-TenantID", "test")
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtMDI4NTRDNkMti00QjlDLUFEQzItRDk5NTc4MTlFRkI3")
  .build();
Response response = client.newCall(request).execute();
```

---

## RESPONSE

```json
{
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC",
  "x-TenantID": "test"
}
```

---

## Authorization Header Values

| Key | Type | Description |
|------|------|-------------|
| **Authorization** | string | Payaza Base 64 encoded merchant's API key |
| **x-TenantID** | string | Connection mode: “live” or “test” |


# Receive Payments

# Virtual Accounts

Virtual Accounts are bank accounts that are created for a specific purpose and last for specified durations.  
Virtual accounts are issued by **Payaza’s partner banks**.  
Our virtual account APIs can be used to create virtual accounts and perform other tasks relevant to virtual accounts.  
Virtual accounts are created to foster **easy payment collections** for your business and platform.

---

## Types of Virtual Accounts

### Static / Reserved Account
This virtual account is created by the merchant and remains valid indefinitely.  
It can be used multiple times and does not expire.

### Dynamic Account
This virtual account is generated for a specific transaction or purpose.  
Please note that a dynamic virtual account remains valid for a **temporary period of time** or **until a payment of the specified amount is received**.  
Upon expiry, a merchant can generate another one for a different purpose.

---

## Virtual Account APIs

### 1. [Create Dynamic Virtual Account](https://docs.payaza.africa/developers/apis/collections/virtual-accounts-new/create-dynamic-virtual-account)
Creates **dynamic virtual accounts**, i.e., temporary virtual accounts.

### 2. [Create Reserved Virtual Account](https://docs.payaza.africa/developers/apis/collections/virtual-accounts-new/create-reserved-virtual-account)
Creates **reserved/static virtual accounts**, i.e., permanent virtual accounts.

### 3. [Get Virtual Account Status](https://docs.payaza.africa/developers/apis/collections/virtual-accounts-new/get-virtual-account-status)
Retrieves the **status** of a reserved/static virtual account.

### 4. [Fund Test Virtual Account](https://docs.payaza.africa/developers/apis/collections/virtual-accounts-new/fund-test-virtual-account)
Used to **fund test collections**.  
> **Note:** This is for **sandbox use only**.

### 5. [Transaction Status Query](https://docs.payaza.africa/developers/apis/collections/virtual-accounts-new/transaction-status-query)
Used to **check the status of a transaction** using the transaction reference.


## Create Dynamic Virtual Account 

## Introduction

This API endpoint allows you to **create dynamic virtual accounts**.  
These accounts have a **duration of 30 minutes** by default.

---

## Arguments

| Name | Type | Required | Description |
|------|------|-----------|-------------|
| `account_name` | string | ✅ | The name assigned to the account being created. |
| `account_type` | string | ✅ | Type of virtual account. `"Dynamic"` is the default value. |
| `bank_code` | string | ✅ | The code of the bank providing the virtual account.<br>• `1067` → 78 FINANCE COMPANY LIMITED<br>• `117` → FIDELITY BANK LIMITED<br>• `140` → GLOBUS BANK LIMITED |
| `bvn` | string | Optional | Bank Verification Number (BVN) of the customer. |
| `has_amount_validation` | boolean | Optional | Determines amount validation rules for the virtual account (allow underpayment, overpayment, or exact amounts).<br>**Available for:** GLOBUS BANK LIMITED and 78 FINANCE COMPANY LIMITED. |
| `account_reference` | string | ✅ | Unique identifier for the transaction. |
| `customer_first_name` | string | ✅ | First name of the customer. |
| `customer_last_name` | string | ✅ | Last name of the customer. |
| `customer_email` | string | ✅ | Email address of the customer. |
| `customer_phone_number` | string | ✅ | Phone number of the customer. |
| `transaction_description` | string | Optional | Description or narration of the transaction. |
| `transaction_amount` | double | ✅ | The amount to be paid in the transaction. |
| `expires_in_minutes` | int | Optional | Duration (in minutes) for which the virtual account remains valid.<br>Default: 30 minutes<br>Min: 15, Max: 480<br>**Available only for:** 78 FINANCE COMPANY LIMITED. |

---

## Endpoint

**Method:** `POST`  
**URL:**  
```
https://api.payaza.africa/live/merchant-collection/merchant/virtual_account/generate_virtual_account/
```

---

## Example Request

```java
OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
  "account_name": "Test DVA",
  "account_type": "Dynamic",
  "bank_code": "1067",
  "bvn": "",
  "has_amount_validation": "true",
  "account_reference": "Ref123456780",
  "customer_first_name": "John",
  "customer_last_name": "Doe",
  "customer_email": "johndoe@gmail.com",
  "customer_phone_number": "07012345678",
  "transaction_description": "Test Description",
  "transaction_amount": "1000",
  "expires_in_minutes": "20"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/merchant-collection/merchant/virtual_account/generate_virtual_account/")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtNjJDREFDRjUtRjI0OC00REY0LUI1RDYtOTM4MTlBQ0NEM0I5")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute(); 
```

---

## Example Response

```json
{
  "message": "Virtual Account generated successfully",
  "data": {
    "account_name": "Payaza(Test DVA)",
    "account_number": "7000009348",
    "account_type": "Dynamic",
    "bank_name": "78 FINANCE COMPANY LIMITED",
    "account_reference": "Ref123456780",
    "transaction_id": 23366137,
    "transaction_amount_payable": 1000,
    "transaction_reference": "Ref123456780",
    "expires_in_minutes": 20
  },
  "success": true
}
```

---

## Notes

- Copy the key from your **dashboard**, encode it in **Base64**, and include it in the `Authorization` header with the `"Payaza"` prefix.  
- Ensure that your account has the proper permissions for **Dynamic Virtual Account creation**.

---


## Authorization Header Values

---

Authorization string

Payaza Base 64 encoded merchant's API key

--- 

```json
POST
          
{ 
 "authorization": Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC,
  
}
```


# Create Reserved Virtual Account

## Introduction
This endpoint creates a **reserved/static virtual account**.

---

## Arguments

| Parameter | Type | Required | Description |
|------------|------|-----------|-------------|
| `account_name` | `string` | ✅ | The name assigned to the account being created. |
| `account_type` | `string` | ✅ | Type of virtual account. Default is `"Static"`. |
| `bank_code` | `string` | ✅ | The code of the bank providing the virtual account.<br>• `1067` – 78 FINANCE COMPANY LIMITED<br>• `117` – FIDELITY BANK LIMITED<br>• `140` – GLOBUS BANK LIMITED |
| `bvn` | `string` | ✅ | Bank Verification Number (BVN) of the customer. |
| `bvn_validated` | `boolean` | ✅ | Indicates whether the BVN has been validated by the merchant. |
| `account_reference` | `string` | ✅ | The unique identifier for the account. |
| `customer_first_name` | `string` | ✅ | First name of the customer. |
| `customer_last_name` | `string` | ✅ | Last name of the customer. |
| `customer_email` | `string` | ✅ | Email address of the customer. |
| `customer_phone_number` | `string` | ✅ | Phone number of the customer. |

---

## POST Endpoint

**POST**  
https://api.payaza.africa/live/merchant-collection/merchant/virtual_account/generate_virtual_account/


---

## Example Request (Java)

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "account_name": "Test Reserved VA",
    "account_type": "Static",
    "bank_code": "1067",
    "bvn": "323212345",
    "bvn_validated": true,
    "account_reference": "accRef123",
    "customer_first_name": "John",
    "customer_last_name": "Doe",
    "customer_email": "johndoe@gmail.com",
    "customer_phone_number": "07012345678"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/merchant-collection/merchant/virtual_account/generate_virtual_account/")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtNjJDREFDRjUtRjI0OC00REY0LUI1RDYtOTM4MTlBQ0NEM0I5")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

```json
{
  "message": "Virtual Account generated successfully",
  "data": {
    "account_name": "Payaza(Test DVA)",
    "account_number": "7000009348",
    "account_type": "Dynamic",
    "bank_name": "78 FINANCE COMPANY LIMITED",
    "account_reference": "Ref123456780",
    "transaction_id": 23366137,
    "transaction_amount_payable": 1000,
    "transaction_reference": "Ref123456780",
    "expires_in_minutes": 20
  },
  "success": true
}
```


| Header          | Type     | Description                               |
| --------------- | -------- | ----------------------------------------- |
| `Authorization` | `string` | Payaza Base64 encoded merchant's API key. |

```json
{
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```


# Get Virtual Account Status

## Introduction

This endpoint allows you to **retrieve the status of a specific virtual account** using its account number.  
It is available **only for static (reserved) virtual accounts**.

---

## Arguments

| Name | Type | Required | Description |
|------|------|-----------|-------------|
| `virtualAccountNumber` | string | ✅ | The virtual account number to be queried. |

---

## Endpoint

**Method:** `GET`  
**URL:**  
https://api.payaza.africa/live/merchant-collection/merchant/virtual_account/detail/virtual_account/{virtualAccountNumber}

---

## Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/merchant-collection/merchant/virtual_account/detail/virtual_account/7000009201")
  .method("GET", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS1RFU1QtNzc2RjBBMzktQ0Q4My00RENDLThCRkItRTgxNDdEMEE2MzVG")
  .build();
Response response = client.newCall(request).execute();
```

### Example Response

```json
{
  "message": "Virtual Account generated successfully",
  "data": {
    "account_name": "Payaza(Test DVA)",
    "account_number": "7000009348",
    "account_type": "Dynamic",
    "bank_name": "78 FINANCE COMPANY LIMITED",
    "account_reference": "Ref123456780",
    "transaction_id": 23366137,
    "transaction_amount_payable": 1000,
    "transaction_reference": "Ref123456780",
    "expires_in_minutes": 20
  },
  "success": true
}
```

### Authorization Header

| Header          | Type   | Description                               |
| --------------- | ------ | ----------------------------------------- |
| `Authorization` | string | Payaza Base64 encoded merchant's API key. |


Example Header

```json
{
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```

# Transaction Status Query (TSQ)

## Introduction
This endpoint is used to check the **status of a transaction** using the **transaction reference**.

---

## Arguments

| Name | Type | Description |
|------|------|--------------|
| `transaction_reference` | `string` | The unique reference ID of the transaction |

---

## HTTP Request

**GET**
**URL:** 
https://api.payaza.africa/live/merchant-collection/transfer_notification_controller/transaction-query?transaction_reference=Ref1234567890

---

### Sample Responses
Before Payment

```json

  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Initialized",
    "sender_name": null,
    "sender_account_number": null,
    "source_bank_name": null,
    "initiated_date": "2024-10-10 18:18:09.117098",
    "current_status_date": null,
    "currency": "NGN",
    "session_id": "",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Awaiting customer to complete payment"
  },
  "success": true
}
```

### After Payment
```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

| Field           | Type     | Description                              |
| --------------- | -------- | ---------------------------------------- |
| `Authorization` | `string` | Payaza Base64 encoded merchant's API key |

```json
{
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}

```

# Fund Test Virtual Account

## Introduction
This endpoint is used to **fund test virtual accounts**.  
> ⚠️ **Note:** This API is for **sandbox/testing purposes only** and should not be used in production environments.

---

## Arguments

| Name | Type | Required | Description |
|------|------|-----------|--------------|
| `account_name` | `string` | ✅ Yes | The name assigned to the virtual account |
| `account_number` | `string` | ✅ Yes | The virtual account number |
| `initiation_transaction_reference` | `string` | ⚙️ Optional | Required for Dynamic Virtual Accounts. Unique identifier for the transaction. Empty for reserved virtual accounts |
| `transaction_amount` | `double` | ✅ Yes | The amount to be paid |
| `currency` | `string` | ✅ Yes | Currency code for the collection (Value: `"NGN"`) |
| `source_account_number` | `string` | ✅ Yes | The payer’s account number |
| `source_account_name` | `string` | ✅ Yes | The payer’s account name |
| `source_bank_name` | `string` | ✅ Yes | The payer’s bank name |

---

## HTTP Request

**POST**
https://api.payaza.africa/live/merchant-collection/payaza/virtual_account/fund_test_virtual_account



---

## Example Request

```java
OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    \"account_name\": \"Payaza(Test DVA)\",
    \"account_number\": \"4030904675\",
    \"initiation_transaction_reference\": \"Ref1234567890\",
    \"transaction_amount\": \"1000\",
    \"currency\": \"NGN\",
    \"source_account_number\": \"0123456789\",
    \"source_account_name\": \"Jill Stones\",
    \"source_bank_name\": \"Test Bank\"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/merchant-collection/payaza/virtual_account/fund_test_virtual_account")
  .method("POST", body)
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```
### Example response 

```json
{
  "message": "Virtual Account funded successfully",
  "data": {
    "account_number": "4030904675",
    "transaction_reference": "Ref1234567890",
    "amount": 1000,
    "currency": "NGN",
    "status": "Success"
  },
  "success": true
}
```

| Field           | Type     | Description                              |
| --------------- | -------- | ---------------------------------------- |
| `Authorization` | `string` | Payaza Base64 encoded merchant's API key |


```json
{
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```


# Card Collection

## Introduction
The **Card Collection APIs** enable you to perform and manage **card-based transactions**.  
They allow you to:
- Initiate payments with debit or credit cards  
- Check transaction statuses  
- Process and verify refunds  
- Confirm whether a card supports **3D Secure (3DS)** authentication  

These APIs are ideal for merchants who need to manage payment workflows securely and efficiently.

---

## Available Endpoints

### 1. [Card Charge](https://docs.payaza.africa/developers/apis/collections/card-collection/card-charge)
This endpoint is used to **initiate a card payment**.

> **Purpose:** Begin a transaction where a customer’s card is charged for a specific amount.

---

### 2. [Check Refund Status](https://docs.payaza.africa/developers/apis/collections/card-collection/refund-status)
This endpoint allows you to **fetch the status of a refunded card transaction**.

> **Purpose:** Determine whether a refund request has been processed, is pending, or has failed.

---

### 3. [Check Transaction Status](https://docs.payaza.africa/developers/apis/collections/card-collection/check-transaction-status)
This endpoint retrieves the **status of a transaction** based on the transaction reference for a particular merchant.

> **Purpose:** Track real-time payment updates and confirm completion or failure of transactions.

---

### 4. [Check 3DS Availability](https://docs.payaza.africa/developers/apis/collections/card-collection/check-3DS-availability)
This endpoint verifies whether a card supports **3D Secure (3DS)** authentication.

> **Purpose:** Determine if a cardholder’s bank requires additional authentication (e.g., OTP or redirect) during checkout for enhanced transaction security.


# Card Collection

# Card Charge

## Introduction
This endpoint is used to **initiate card payments** for merchants using the Payaza platform.  
It supports **3D Secure (3DS) authentication**, which is required for processing 3DS-enabled cards.

---

## 3DS Authentication HTML

This HTML snippet is used for **3DS card authentication**:

```
html
<body>
    <div id='threedsChallengeRedirect' xmlns='http://www.w3.org/1999/html' style='height: 100vh'>
        <form id='threedsChallengeRedirectForm' method='POST' action='' target='challengeFrame'>
            <input type='hidden' name='creq' id="creq" value=''/>
        </form>
        <iframe id='challengeFrame' name='challengeFrame' width='100%' height='100%'></iframe>
    </div>

    <script>
        // Card details
        const cardNumber = "4187451844054629";
        const expiryMonth = "07";
        const expiryYear = "32";
        const securityCode = "100";

        // Prepare request
        var myHeaders = new Headers();
        myHeaders.append("Authorization", "Payaza UFo3OC1QS0xJhsksksMjFFNEYtQ0VCNy00MjAzL4MDktQkU1NEM3NDY1RDRB");
        myHeaders.append("Content-Type", "application/json");

        var raw = JSON.stringify({
            "service_type": "Account",
            "service_payload": {
                "request_application": "Payaza",
                "application_module": "USER_MODULE",
                "application_version": "1.0.0",
                "request_class": "UsdCardChargeRequest",
                "first_name": "John",
                "last_name": "Doe",
                "email_address": "johndoe@email.com",
                "phone_number": "09058i983106",
                "amount": 10,
                "transaction_reference": "PL-1KBPSCJCR" + Math.floor((Math.random() * 10000000) + 1),
                "currency": "USD",
                "description": "Test for 3DS",
                "card": {
                    "expiryMonth": expiryMonth,
                    "expiryYear": expiryYear,
                    "securityCode": securityCode,
                    "cardNumber": cardNumber
                }
            }
        });

        var requestOptions = {
            method: 'POST',
            headers: myHeaders,
            body: raw,
            redirect: 'follow'
        };

        fetch("https://api.payaza.africa/live/card/card_charge/", requestOptions)
            .then(response => response.text())
            .then(result => {
                result = JSON.parse(result);
                if (result.statusOk) {
                    const creq = document.getElementById("creq");
                    creq.value = result.formData;
                    const form = document.getElementById("threedsChallengeRedirectForm");
                    form.setAttribute("action", result.threeDsUrl);
                    form.submit();
                } else {
                    console.log("Error found", result.debugMessage)
                    alert("Payment Failed: " + result.debugMessage)
                }
            }).catch(error => {
                console.log("Error", error)
                alert("Exception Error: " + error.debugMessage)
            });

        // Internal Payment Notification
        window.addEventListener("message", (event) => {
            try {
                const response = JSON.parse(event.data);
                if (response.statusOk !== undefined) {
                    if (response.statusOk && response.paymentCompleted) {
                        alert("Payment Successful")
                    } else {
                        alert("Payment Failed")
                    }
                }
            } catch(error) {
                console.log("Error from Parsing JSON", error)
            }
        });
    </script>
</body>
```

| Name                    | Type   | Required | Description                                |
| ----------------------- | ------ | -------- | ------------------------------------------ |
| `service_payload`       | array  | ✅        | Contains the card payment details.         |
| `first_name`            | string | ✅        | Customer's first name.                     |
| `last_name`             | string | ✅        | Customer's last name.                      |
| `email_address`         | string | ✅        | Customer email address.                    |
| `phone_number`          | string | ✅        | Customer phone number.                     |
| `amount`                | double | ✅        | Amount to be charged.                      |
| `transaction_reference` | string | ✅        | Unique transaction ID (max 15 characters). |
| `currency`              | string | ✅        | Currency code (base currency is USD).      |
| `description`           | string | Optional | Description for the payment.               |
| `card`                  | object | ✅        | Card details.                              |
| `expiryMonth`           | string | ✅        | Card expiry month.                         |
| `expiryYear`            | string | ✅        | Card expiry year.                          |
| `securityCode`          | string | ✅        | Card CVV.                                  |
| `cardNumber`            | string | ✅        | Card number.                               |


---

## Test Cards

| Card Number      | Expiry | CVV | 3DS |
| ---------------- | ------ | --- | --- |
| 4012000033330026 | 01/39  | 100 | N   |
| 4508750015741019 | 01/39  | 100 | Y   |

    Note: These cards are valid only in Test Mode.

---

Endpoint

Method: POST
URL: https://api.payaza.africa/live/card/card_charge/

```java
OkHttpClient client = new OkHttpClient().newBuilder().build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
  \"service_payload\": {
    \"first_name\": \"John\",
    \"last_name\": \"Doe\",
    \"email_address\": \"johndoe@gmail.com\",
    \"phone_number\": \"0939344404\",
    \"amount\": 0.01,
    \"transaction_reference\": \"T13501973673737\",
    \"currency\": \"USD\",
    \"description\": \"Test\",
    \"card\": {
      \"expiryMonth\": \"10\",
      \"expiryYear\": \"26\",
      \"securityCode\": \"686\",
      \"cardNumber\": \"4865550017193640\"
    }
  }
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/card/card_charge/")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();

```

### Response Parameters

| Parameter      | Success Status         | Failed Status                                             | Description                                  |
| -------------- | ---------------------- | --------------------------------------------------------- | -------------------------------------------- |
| `StatusOk`     | true                   | false                                                     | Indicates if the transaction was successful. |
| `message`      | Approved               | Transaction Failed                                        | General transaction message.                 |
| `debugMessage` | Transaction Successful | Error details (invalid card, duplicate transaction, etc.) | Detailed description for debugging.          |
| `3DS`          | NON-3DS                | N/A                                                       | Indicates if 3D Secure is used.              |


### Example Response

#### 3DS

```json
 {
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

#### Non-3DS
```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

### Callback URL Response

#### Successful

```json
array (
  'payload' => '{
  "statusOk": true,
  "message": "Approved",
  "debugMessage": "Transaction+Successful", 
  "description": "TEST",
  "descriptor": "+",
  "waitForNotification": true,
  "transactionReference": "P1KRDXCD5630",
  "customerReference": "P1KRDXCD5630",
  "do3dsAuth": false,
  "paymentCompleted": true,
  "amountPaid": 24,
  "valueAmount": 23.664,
  "payer_name": "John+Doe",
  "source_bank_name": "VISA",
  "payment_date": "2024-03-11+19:49:43.575475932",
  "created_at": "2024-03-11+19:49:43.575475932",
  "rrn": "519219078440"
}',
)
```

#### Unsuccessful

```json
array (
  'payload' => '{
  "statusOk": false,
  "message": "Transaction Failed",
  "debugMessage": "DO_NOT_PROCEED",
  "waitForNotification": false,
  "transactionReference": "P1KXVLO138",
  "do3dsAuth": false,
  "paymentCompleted": false,
  "amountPaid": 0,
  "valueAmount": 0
}',
)
```

### Authorization Header

| Name            | Value                                                |
| --------------- | ---------------------------------------------------- |
| `Authorization` | Base64-encoded Payaza API key with `"Payaza"` prefix |

```json
{ 
 "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```


# Check Card Transaction Status

## Introduction
This endpoint retrieves the **status of a card transaction** corresponding to the provided transaction reference. It is used for monitoring payment completion or failure.

---

## Arguments

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `service_payload` | array | ✅ | Contains the transaction details. |
| `transaction_reference` | string | ✅ | The unique reference ID of the transaction to be checked. |

---

## Endpoint

**Method:** `POST`  
**URL:**  https://api.payaza.africa/live/card/card_charge/transaction_status

## Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
  "service_payload": {
    "transaction_reference": "A12345"
  }
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/card/card_charge/transaction_status")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

### Response

The response contains the current status of the transaction, including success/failure details, payment completion, and associated metadata.

---

### Authorization Header

| Name            | Value                                                |
| --------------- | ---------------------------------------------------- |
| `Authorization` | Base64-encoded Payaza API key with `"Payaza"` prefix |

```json
{ 
 "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```

# Check Card Refund Status

## Introduction
This endpoint retrieves the **status of a refunded card payment**. It allows merchants to verify whether a refund has been successfully processed.

---

## Arguments

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `service_payload` | array | ✅ | Contains the refund transaction details. |
| `refund_transaction_reference` | string | ✅ | The unique reference ID of the refunded transaction. This can be found in the response of the **Card Charge Refund API**. |

---

## Endpoint

**Method:** `POST`  
**URL:** https://api.payaza.africa/live/card/card_charge/refund_status


### Request 

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
  "service_payload": {
    "refund_transaction_reference": "RF20231112-Q232USD"
  }

}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/card/card_charge/refund_status")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

---

### Response

The response contains details about the refunded transaction, including refund status, amount refunded, and success indicator.

---

### Authorization Header 

| Name            | Value                                                |
| --------------- | ---------------------------------------------------- |
| `Authorization` | Base64-encoded Payaza API key with `"Payaza"` prefix |


```json
{ 
 "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}

```

# Check 3DS Availability

## Introduction
This API endpoint verifies whether a card supports **3D Secure (3DS)** authentication. This helps merchants determine if additional 3DS verification is required for a card transaction.

---

## Arguments

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `card_number` | string | ✅ | The card number to be checked. |
| `currency` | string | ✅ | The currency code for the transaction. |

---

## Endpoint

**Method:** `POST`  
**URL:** https://api.payaza.africa/live/card/card_charge/check_3ds_availability

## Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
"card_number": "4860079468120",
"currency": "NGN"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/card/card_charge/check_3ds_availability")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

## Example Response

### 3DS

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

### NON-3DS
```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

---

## Authorization Header

| Name            | Value                                                |
| --------------- | ---------------------------------------------------- |
| `Authorization` | Base64-encoded Payaza API key with `"Payaza"` prefix |

```json
{ 
 "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}

```


# Refunds and Chargebacks

## Introduction
The Refunds and Chargeback APIs allow merchants to **initiate refunds**, **view refund and chargeback history**, and **manage chargeback requests** efficiently.

---

## Refunds and Chargeback APIs

| API | Description |
|-----|-------------|
| [Initiate Refund V2](https://docs.payaza.africa/developers/apis/collections/refunds-and-chargebacks/initiate-refund) | Initiates a refund for a card payment. |
| [Fetch Refund History V2](https://docs.payaza.africa/developers/apis/collections/refunds-and-chargebacks/fetch-refund-history) | Retrieves the history of refunded card payments. |
| [Accept or Reject Chargeback](https://docs.payaza.africa/developers/apis/collections/refunds-and-chargebacks/accept-or-reject-chargeback) | Allows merchants to **accept** or **reject** a chargeback request. |
| [Chargeback Request](https://docs.payaza.africa/developers/apis/collections/refunds-and-chargebacks/chargeback-request) | Fetches chargeback requests submitted against the merchant. |
| [Chargeback Transaction History](https://docs.payaza.africa/developers/apis/collections/refunds-and-chargebacks/chargeback-transaction-history) | Retrieves all chargeback transactions for the merchant. |


# Initiate Refund V2

## Introduction
This endpoint is used to **initiate a refund** for a specific transaction.

---

## Arguments

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `transaction_reference` | string | ✅ | The unique identifier of the transaction to be refunded. |
| `refund_amount` | double | ✅ | The amount to be refunded. |
| `refund_reason` | string | Optional | A brief reason for the refund. |

---

## Endpoint

**Method:** `POST`  
**URL:** https://api.payaza.africa/live/refund-chargeback/refund/merchant/api/refund


## Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
  "transaction_reference": "APRQWE1001",
  "refund_amount": 0.05,
  "refund_reason": "Refund"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/refund-chargeback/refund/merchant/api/refund")
  .method("POST", body)
  .addHeader("Content-Type", "application/json")
  .addHeader("Accept", "application/json")
  .addHeader("Authorization", "Payaza UFo3OC1QSJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtMCQUI4MTJBNzdC")
  .build();
Response response = client.newCall(request).execute();
```

# Authorization

| Header          | Value                                    |
| --------------- | ---------------------------------------- |
| `Authorization` | Payaza Base64 encoded merchant's API key |



# Fetch Refund History V2

## Introduction
This endpoint allows a merchant to **fetch refund transaction history**. The data can be filtered by date range, currency, or refund status.

---

## Arguments

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `fromdate` | string | Optional | Start date for filtering in `YYYY-MM-DD` format. |
| `todate` | string | Optional | End date for filtering in `YYYY-MM-DD` format. |
| `currency` | string | Optional | ISO 4217 alpha-3 currency code (e.g., NGN, USD). |
| `page` | int | ✅ | Page number of the results. |
| `size` | int | ✅ | Number of transactions per page. |
| `refund_status` | string | Optional | Filter by status: `"Success"` or `"Initialized"`. |

---

## Endpoint

**Method:** `GET`  
**URL:**  https://api.payaza.africa/live/refund-chargeback/refund/merchant/api/refund_history?from=&to=&currency=USD&page=1&size=10&refund_status=Success

## Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/refund-chargeback/refund/merchant/api/refund_history?from=&to=&currency=USD&page=1&size=10&refund_status=Success")
  .method("GET", body)
  .addHeader("Accept", "application/json")
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
  .build();
Response response = client.newCall(request).execute();
```

### Authorization Header Values

| Header          | Value                                    |
| --------------- | ---------------------------------------- |
| `Authorization` | Payaza Base64 encoded merchant's API key |


```json 
{
"authorization":"Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```


# Accept or Reject Chargeback

## Introduction
This endpoint allows a merchant to **accept or decline a chargeback request**.

---

## Arguments

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `action` | string | ✅ | Specify the action: `"accept"` or `"decline"`. |
| `chargeback_fk` | int | ✅ | Unique identifier of the chargeback (from Chargeback request API response). |
| `comment` | string | Optional | Brief note explaining the action. |
| `evidence_url` | string | ✅ | URL of the evidence; must be accessible. |

---

## Endpoint

**Method:** `POST`  
**URL:** https://api.payaza.africa/live/refund-chargeback/chargeback/merchant/api/accept_reject_chargeback

## Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
.build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "action": "accept",
    "chargeback_fk": 39,
    "comment": "Test",
    "evidence_url": "https://evidence.url"
}");
Request request = new Request.Builder()
.url("https://api.payaza.africa/live/refund-chargeback/chargeback/merchant/api/accept_reject_chargeback")
.method("POST", body)
.addHeader("Content-Type", "application/json")
.addHeader("Accept", "application/json")
.addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
.build();
Response response = client.newCall(request).execute();
```

## Possible Responses 

### Accepted

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

### Rejected 

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

### Authorization Header

| Header          | Value                                    |
| --------------- | ---------------------------------------- |
| `Authorization` | Payaza Base64 encoded merchant's API key |

```json
{
"authorization":"Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```


# Chargeback Requests

## Introduction
This endpoint allows a merchant to **fetch chargeback requests**.

---

## Arguments

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `transaction_reference` | string | Optional | Filter by a specific transaction reference. |
| `fromdate` | string | Optional | Start date to filter data (YYYY-MM-DD). |
| `todate` | string | Optional | End date to filter data (YYYY-MM-DD). |
| `page` | int | ✅ | Page number for pagination. |
| `size` | int | ✅ | Number of transactions per page. |

---

## Endpoint

**Method:** `GET`  
**URL:** https://api.payaza.africa/live/refund-chargeback/chargeback/merchant/api/chargeback_requests?transaction_reference=&from=&to=&page=1&size=10

## Example Request

```java
OkHttpClient client = new OkHttpClient().newBuilder()
.build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
.url("https://api.payaza.africa/live/refund-chargeback/chargeback/merchant/api/chargeback_requests?transaction_reference=&page=1&size=10&from=&to=")
.method("GET", body)
.addHeader("Accept", "application/json")
.addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
.build();
Response response = client.newCall(request).execute();
```

## Response

The API returns a JSON payload containing the requested chargeback request data.

## Authorization Headers

| Header          | Value                                    |
| --------------- | ---------------------------------------- |
| `Authorization` | Payaza Base64 encoded merchant's API key |


# Chargeback Transaction History

## Introduction
This endpoint allows a merchant to **fetch all chargeback transactions**.

---

## Arguments

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `transaction_reference` | string | Optional | Filter by a specific transaction reference. |
| `fromdate` | string | Optional | Start date to filter data (YYYY-MM-DD). |
| `todate` | string | Optional | End date to filter data (YYYY-MM-DD). |
| `page` | int | ✅ | Page number for pagination. |
| `size` | int | ✅ | Number of transactions per page. |

---

## Endpoint

**Method:** `GET`  
**URL:** https://api.payaza.africa/live/refund-chargeback/chargeback/merchant/api/chargeback_transaction_history?transaction_reference=&from=&to=&page=1&size=10

## Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
.build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
.url("https://api.payaza.africa/live/refund-chargeback/chargeback/merchant/api/chargeback_transaction_history?transaction_reference=&from=&to=&page=1&size=10")
.method("GET", body)
.addHeader("Accept", "application/json")
.addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC")
.build();
Response response = client.newCall(request).execute();
```

## Authorization Header

| Header          | Value                                    |
| --------------- | ---------------------------------------- |
| `Authorization` | Payaza Base64 encoded merchant's API key |


```json
{
"authorization":"Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```


# Momo Collections

## Introduction
The **Momo Collection APIs** allow merchants to process mobile money collections in **GHS, KES, UGX, XAF, and TZS**, check the transaction status, and fund test accounts. These APIs are essential for mobile money payment processing.

---

## Momo Collection APIs

| API | Description |
|-----|-------------|
| **[Momo Process Collection](https://docs.payaza.africa/developers/apis/collections/momo-collections/momo-process-collection)** | Initiates a mobile money collection. |
| **[Transaction Status Query](https://docs.payaza.africa/developers/apis/collections/momo-collections/transaction-status-query)** | Retrieves the status of a mobile money transaction using a transaction reference. |
| **[Test Account Funding](https://docs.payaza.africa/developers/apis/collections/momo-collections/test-account-funding)** | Funds test accounts for mobile money collections. This API is for testing purposes only. |

---

## The Collection Flow
The typical Momo collection flow involves initiating a collection, waiting for confirmation, and processing notifications. Webhook notifications are used to update transaction statuses in real-time.

<img width="1000" height="694" alt="Screenshot 2025-11-12 at 13 20 59" src="https://gist.github.com/user-attachments/assets/4d5d2022-e4ef-4c6a-bf67-2283bfd67ddc" />

---

## Example: KES Momo Webhook Notification

```json
{
  "transaction_reference": "LAX1234",
  "transaction_status": "Funds Received",
  "virtual_account_number": "",
  "transaction_fee": 1,
  "amount_received": 50.5,
  "initiated_date": "2024-09-08 20:24:09",
  "current_status_date": "2024-09-08 20:24:48",
  "received_from": {
      "account_name": "John Doe",
      "account_number": "233123456789",
      "bank_name": "N/A"
  },
  "channel": "KE_MOBILEMONEY",
  "currency_code": "KES",
  "branch": false,
  "session_id": "P-C-202498-779AB97963",
  "status": "Completed"
}
```

# Momo Process Collection

## Introduction
This endpoint is used to **initiate mobile money collections**. 

> **Note:**  
> - Payaza MOMO collection codes can be accessed [here](https://docs.google.com/spreadsheets/d/1BOGf_mSLS6rGNm1vn3cO4A2vW9oqZVGI_PxxxXnMK9o/edit?usp=sharing).  
> - Collections to countries outside Nigeria are available upon request. To gain access, email **support@payaza.africa**. Access will be granted once approved by the Payaza team.

---

## Arguments

| Parameter | Type | Description |
|-----------|------|-------------|
| `amount` | double | The amount to be paid. |
| `customer_number` | string | The mobile money account number to be charged. <br> - Ghana, Uganda, Kenya, Tanzania, Cameroon: 12 digits (including country code) <br> - Sierra Leone: 11 digits (including country code) |
| `transaction_reference` | string | Unique identifier of the transaction. |
| `transaction_description` | string | Description of the transaction. |
| `customer_bank_code` | string | Customer’s mobile money code. |
| `currency_code` | string | Currency code (GHS, KES, TZS, UGX, SLE, XAF). |
| `customer_email` | string | Email address of the customer. |
| `customer_first_name` | string | Customer’s first name. |
| `customer_last_name` | string | Customer’s last name. |
| `customer_phone_number` | string | Customer’s phone number. |
| `country_code` | string | ISO Country Code (ISO 3166-1 alpha-2): GH-Ghana, TZ-Tanzania, UG-Uganda, KE-Kenya, SL-Sierra Leone, CM-Cameroon |

---

## POST Endpoint: https://api.payaza.africa/live/subsidiary/collections/v1/process-collection

## Example Request

```java
OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "amount": 200.27,
    "customer_number": "233123456789",
    "transaction_reference": "UDHJQ012340",
    "transaction_description": "Test Payment",
    "customer_bank_code": "SAFKEN",
    "currency_code": "KES",
    "customer_email": "bigmaitre@blondmail.com",
    "customer_first_name": "Robert",
    "customer_last_name": "Stones",
    "customer_phone_number": "012345678901",
    "country_code": "KE" 
}");

Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/subsidiary/collections/v1/process-collection")
  .method("POST", body)
  .addHeader("X-TenantID", "test")
  .addHeader("X-ProductID", "app")
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTBNzdC")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

---

### Authorization Header Values

| Header          | Value                             | Description                       |
| --------------- | --------------------------------- | --------------------------------- |
| `Authorization` | Base64 encoded merchant's API key | Required for authentication       |
| `X-TenantID`    | test                              | Connection mode: "live" or "test" |
| `X-ProductID`   | app                               | Product identifier                |

```json
{
"authorization":"Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```


# Mobile Money Transaction Status

## Introduction
This endpoint retrieves the **status of a mobile money transaction** corresponding to the provided transaction reference.

---

## Arguments

| Parameter | Type | Description |
|-----------|------|-------------|
| `transaction_reference` | string | The unique identifier of the transaction. |
| `country_code` | string | ISO Country Code (ISO 3166-1 alpha-2): GH-Ghana, TZ-Tanzania, UG-Uganda, KE-Kenya. |

---

## GET Endpoint: https://api.payaza.africa/live/subsidiary/collections/v1/check-status?transaction_reference=UDH012345&country_code=KE


## Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/subsidiary/collections/v1/check-status?transaction_reference=UDH012345&country_code=KE")
  .method("GET", body)
  .addHeader("X-TenantID", "test")
  .addHeader("X-ProductID", "app")
  .addHeader("Content-Type", "application/json")
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTBNzdC")
  .build();
Response response = client.newCall(request).execute();

```

### Response Example

#### Before Payment

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

#### After Payment 

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

### Authorization Header Values 

| Header          | Value                             | Description                       |
| --------------- | --------------------------------- | --------------------------------- |
| `Authorization` | Base64 encoded merchant's API key | Required for authentication       |
| `X-TenantID`    | test                              | Connection mode: “live” or “test” |
| `X-ProductID`   | app                               | Product identifier                |


```json
{
"authorization":"Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```


# Test Account Funding

## Introduction
This endpoint is used to **fund test accounts**. Please note that this API is strictly for **testing purposes**.

---

## Arguments

| Parameter | Type | Description |
|-----------|------|-------------|
| `transaction_reference` | string | The unique identifier of the transaction. |
| `country_code` | string | ISO Country Code (ISO 3166-1 alpha-2): GH-Ghana, TZ-Tanzania, UG-Uganda, KE-Kenya. |

---

## POST Endpoint: https://api.payaza.africa/live/subsidiary/funding/v1/process-collection

## Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "transaction_reference": "UDH0123453",
    "country_code": "KE"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/subsidiary/funding/v1/process-collection")
  .method("POST", body)
  .addHeader("X-TenantID", "test")
  .addHeader("X-ProductID", "app")
  .addHeader("Content-Type", "application/json")
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTBNzdC")
  .build();
Response response = client.newCall(request).execute();
```

### Authorization Header Values

| Header          | Value                             | Description                       |
| --------------- | --------------------------------- | --------------------------------- |
| `Authorization` | Base64 encoded merchant's API key | Required for authentication       |
| `X-TenantID`    | test                              | Connection mode: “live” or “test” |
| `X-ProductID`   | app                               | Product identifier                |


```json
{
"authorization":"Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```


# ZAR Collections

Our ZAR Collection APIs consist of endpoints that **process ZAR collections** and **check the transaction status** of a ZAR collection.

---

## ZAR Collection APIs

### 1. [ZAR Process Collection](https://docs.payaza.africa/developers/apis/collections/zar-collections/zar-process-collection)
This endpoint is used to **initiate a ZAR collection**.

### 2. [Transaction Status Query](https://docs.payaza.africa/developers/apis/collections/zar-collections/zar-process-collection)
This endpoint retrieves the **status of a ZAR collection** corresponding to the provided transaction reference.

---

## Example of ZAR Webhook Notification

```json
{
  "transaction_reference": "Xli13291012308",
  "transaction_status": "Funds Received",
  "virtual_account_number": "",
  "transaction_fee": 3,
  "amount_received": 100,
  "initiated_date": "2025-07-01 03:55:11",
  "current_status_date": "2025-07-01 03:57:01",
  "received_from": {
    "account_name": "John Doe",
    "account_number": "null",
    "bank_name": "N/A"
  },
  "status": "Completed",
  "session_id": "175168411144840",
  "channel": "EFT_COLLECTIONS",
  "branch": false,
  "currency_code": "ZAR",
  "business_fk": 33145
}
```


---
# ZAR Process Collection

This endpoint initiates a **ZAR collection**.

> **Note:**  
> Collections to countries other than Nigeria are available upon request. To request access, email **support@payaza.africa**. Access will be granted after review and approval by the Payaza team.

---

## Arguments

| Argument                  | Type     | Description                                                                 |
|----------------------------|---------|-----------------------------------------------------------------------------|
| `amount`                   | double  | The amount to be paid                                                        |
| `transaction_reference`    | string  | The unique identifier of the transaction                                     |
| `transaction_description`  | string  | Description of the transaction                                              |
| `customer_bank_code`       | string  | Bank code for ZAR collections, e.g., `EFTZAR`                                |
| `currency_code`            | string  | Currency code, must be `ZAR`                                                |
| `customer_email`           | string  | Email address of the customer                                               |
| `customer_first_name`      | string  | First name of the customer                                                  |
| `customer_last_name`       | string  | Last name of the customer                                                   |
| `redirect_url`             | string  | URL the user is redirected to on successful payment                        |
| `cancel_url`               | string  | URL the user is redirected to if payment is canceled                        |
| `error_url`                | string  | URL the user is redirected to if payment fails                              |
| `customer_phone_number`    | string  | Phone number of the customer                                               |
| `country_code`             | string  | ISO Country Code (ISO 3166-1 alpha-2), e.g., `ZA`)                          |

---

## POST Endpoint

**URL:** https://api.payaza.africa/live/subsidiary/collections/v1/process-collection

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "amount":100,
    "transaction_reference": "tran2232",
    "transaction_description": "Test Payment",
    "customer_bank_code": "EFTZAR",
    "currency_code": "ZAR",
    "customer_email": "johndoe@gmail.com",
    "customer_first_name": "Tshabalala",
    "customer_last_name": "Doe",
    "redirect_url":"https://redirecturl.com",
    "cancel_url":"https://cancelurl.com",
    "error_url":"https://errorurl.com",
    "customer_phone_number": "27113785456",
    "country_code": "ZA"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/subsidiary/collections/v1/process-collection")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS1RFU1QtN0IyM0NGNTUtQzVCQS00QzU4LTkwNDYtRDI5RTJCNTVFNjc4")
  .addHeader("X-TenantID", "test")
  .addHeader("X-ProductID", "app")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();

```

### Authorization Header Values: 

| Header          | Value Description                        |
| --------------- | ---------------------------------------- |
| `Authorization` | Payaza Base64 encoded merchant's API key |
| `X-TenantID`    | Connection mode: “live” or “test”        |
| `X-ProductID`   | “app”                                    |

```json
{ 
  "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}

```
---

# ZAR Transaction Status Query

This endpoint retrieves the status of a **ZAR mobile money transaction** corresponding to the provided transaction reference.

---

## Arguments

| Argument                  | Type    | Description                                                                 |
|----------------------------|--------|-----------------------------------------------------------------------------|
| `transaction_reference`    | string | The unique identifier of the transaction                                     |
| `country_code`             | string | ISO Country Code (ISO 3166-1 alpha-2), e.g., `ZA`)                          |

---

## GET Endpoint

**URL:** https://api.payaza.africa/live/subsidiary/collections/v1/check-status?transaction_reference=UDH012345&country_code=ZA

## Example Request 

```java


OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/subsidiary/collections/v1/check-status?transaction_reference=UDH012345&country_code=ZA")
  .method("GET", body)
  .addHeader("X-TenantID", "test")
  .addHeader("X-ProductID", "app")
  .addHeader("Content-Type", "application/json")
  .addHeader("Authorization", "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTBNzdC")
  .build();
Response response = client.newCall(request).execute();

```

### Responses

#### before Payment

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

#### after Payment

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

### Authorization Header Values 

| Header          | Description                              |
| --------------- | ---------------------------------------- |
| `Authorization` | Payaza Base64 encoded merchant's API key |
| `X-TenantID`    | Connection mode: “live” or “test”        |
| `X-ProductID`   | “app”                                    |

```json
{ 
 "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```

# XOF Momo

The **XOF Mobile Money APIs** allow merchants to process XOF collections, handle OTP verification, check transaction status, and test XOF collections for integration purposes.  

---

## XOF Momo APIs

### 1. [XOF Process Collection](https://docs.payaza.africa/developers/apis/collections/xof-momo/xof-process-collection)
This endpoint initiates an XOF mobile money collection.

### 2. [XOF Process OTP](https://docs.payaza.africa/developers/apis/collections/xof-momo/xof-process-otp)
This endpoint processes the One-Time Password (OTP) required for completing certain XOF transactions.

### 3. [Transaction Status Query](https://docs.payaza.africa/developers/apis/collections/xof-momo/transaction-status-query)
This endpoint retrieves the status of a mobile money transaction corresponding to the provided transaction reference.

### 4. [Test Account Funding](https://docs.payaza.africa/developers/apis/collections/xof-momo/test-account-funding)
This endpoint funds test accounts for integration purposes. **Note:** This API is only for testing XOF collections.

---

## Collection Flow

The typical XOF Momo collection flow involves initiating a collection, optionally verifying via OTP, and confirming the transaction status via webhook notifications.

<img width="1020" height="742" alt="Screenshot 2025-11-12 at 14 03 04" src="https://gist.github.com/user-attachments/assets/4c2dcdba-e7b8-4f83-bc4a-b80d29d0781b" />


---

## Example XOF Momo Webhook Notification

```json
{
  "transaction_reference": "MOC0128",
  "transaction_status": "Funds Received",
  "virtual_account_number": "",
  "transaction_fee": 0,
  "amount_received": 2000,
  "initiated_date": "2024-09-08 20:42:26",
  "current_status_date": "2024-09-08 20:43:10",
  "received_from": {
    "account_name": "John Doe",
    "account_number": "+2250004768076",
    "bank_name": "N/A"
  },
  "channel": "CIV_COLLECTIONS",
  "currency_code": "XOF",
  "branch": false,
  "session_id": "P-C-202498-A47AF41072",
  "status": "Completed"
}
```


# XOF Process Collection

This endpoint initiates an XOF mobile money collection.  

**For testing purposes:**
- Use `2250004768076` for **No OTP Required** option.
- Use `2251114462945` for **OTP Required** option.
- Use OTP value `4567` for tests.

---

## Note

- Payaza XOF collection codes can be accessed [here](#).  
- Collections to countries other than Nigeria are available upon request. Send an email to `support@payaza.africa` to request access.  
- For live transactions, provide your webhook URL on the dashboard.

---

## Arguments

| Argument | Type | Description |
|----------|------|-------------|
| `amount` | double | The amount to be paid |
| `customer_number` | string | Mobile money account number to be charged. <br>- Côte D'Ivoire: 13 digits (including country code) <br>- Benin Republic: 13 digits (including country code), add “01” prefix for successful transactions (format: 229 01 12345678) |
| `transaction_reference` | string | Unique identifier of the transaction |
| `transaction_description` | string | Description of the transaction |
| `customer_bank_code` | string | Customer’s bank code |
| `currency_code` | string | Currency code |
| `customer_email` | string | Email address of the customer |
| `customer_first_name` | string | First name of the customer |
| `customer_last_name` | string | Last name of the customer |
| `customer_phone_number` | string | Phone number of the customer |
| `country_code` | string | ISO Country Code (ISO 3166-1 alpha-2) |
| `redirect_url` | string, optional | URL the customer is redirected to (required for Wave) |

---

## POST Endpoint: https://api.payaza.africa/live/subsidiary/collections/v1/process-collection

### Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "amount": 350,
    "customer_number": "2251114462945",
    "transaction_reference": "MOCIV0009383",
    "transaction_description": "Test Payment",
    "customer_bank_code": "MOMCIV",
    "currency_code": "XOF",
    "customer_email": "johndoe@yahoo.com",
    "customer_first_name": "John",
    "customer_last_name": "Doe",
    "customer_phone_number": "2251114462945",
    "country_code": "CI",
    "redirect_url": "{{redirecturl}}"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/subsidiary/collections/v1/process-collection")
  .method("POST", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS1RFU1QtRkVERDJDMzUtQTc2Ni00Q0MzLUFENjUtNjI0MzQwODJERjk0")
  .addHeader("X-TenantID", "test")
  .addHeader("X-ProductID", "app")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

### Responses

#### OTP Required

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

#### No OTP Required

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```
#### Wave Response: 

```json
```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

### Authorization Header Values: 

| Header          | Description                               |
| --------------- | ----------------------------------------- |
| `Authorization` | Payaza Base 64 encoded merchant's API key |
| `X-TenantID`    | `test` (Connection mode: live or test)    |
| `X-ProductID`   | `app`                                     |


```json
{
  "Authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTBNzdC",
  "X-TenantID": "test",
  "X-ProductID": "app"
}
```

# XOF Process OTP

This endpoint processes the One Time Password (OTP) for XOF mobile money collections.  

**For testing purposes:**
- Use `2251114462945` for the OTP Required option.
- Use OTP value `4567` for your tests.

---

## Arguments

| Argument | Type | Description |
|----------|------|-------------|
| `payment_token` | string | Payment token retrieved from the **Process Collection** API response |
| `otp_code` | string | One Time Password sent to the customer |
| `payee` | string | Mobile money account number to be charged |
| `network_provider` | string | Network provider of the payee |
| `transaction_reference` | string | Unique identifier of the transaction |
| `transaction_channel` | string | Customer’s mobile money code |
| `country_code` | string | ISO Country Code (ISO 3166-1 alpha-2) |

---

## POST Endpoint: https://api.payaza.africa/live/subsidiary/collections/v1/process-otp

### Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "payment_token": "45c3d124-7831-4f97-8844-581bf497a20b",
    "otp_code": "4567",
    "payee": "+2251114462945",
    "payment_method": "ORANGE_CI",
    "transaction_reference": "MOCIV0009383",
    "transaction_channel": "MOMCIV",
    "country_code": "CI"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/subsidiary/collections/v1/process-otp")
  .method("POST", body)
  .addHeader("X-TenantID", "test")
  .addHeader("X-ProductID", "app")
  .addHeader("Authorization", "Payaza UFo3OC1QS1RFU1QtNzc2RjBBMzktQ0Q4My00RENDLThCRkItRTgxNDdEMEE2MzVG")
  .addHeader("Content-Type", "application/json")
  .build();
Response response = client.newCall(request).execute();
```

### Authorization Header Values

| Header          | Description                               |
| --------------- | ----------------------------------------- |
| `Authorization` | Payaza Base 64 encoded merchant's API key |
| `X-TenantID`    | `test` (Connection mode: live or test)    |
| `X-ProductID`   | `app`                                     |

```json
{
  "Authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTBNzdC",
  "X-TenantID": "test",
  "X-ProductID": "app"
}
```

# XOF Transaction Status Query

This endpoint retrieves the mobile money transaction status corresponding to the provided transaction reference.

---

## Arguments

| Argument | Type | Description |
|----------|------|-------------|
| `transaction_reference` | string | Unique identifier of the transaction |
| `country_code` | string | ISO Country Code (ISO 3166-1 alpha-2) |

---

## GET Endpoint: https://api.payaza.africa/live/subsidiary/collections/v1/check-status?transaction_reference=MOCIV0009383&country_code=CI

### Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/subsidiary/collections/v1/check-status?transaction_reference=MOCIV0009383&country_code=CI")
  .method("GET", body)
  .addHeader("X-TenantID", "test")
  .addHeader("X-ProductID", "app")
  .addHeader("Content-Type", "application/json")
  .addHeader("Authorization", "Payaza UFo3OC1QS1RFU1QtNzc2RjBBMzktQ0Q4My00RENDLThCRkItRTgxNDdEMEE2MzVG")
  .build();
Response response = client.newCall(request).execute();
```

### Example Response 

#### Before Payment 

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

#### After Payment

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": "Ref1234567890",
    "amount_received": 1000,
    "transaction_fee": 0,
    "transaction_status": "Completed",
    "sender_name": "Jill Stones",
    "sender_account_number": "0123456789",
    "source_bank_name": null,
    "initiated_date": "2024-10-10 19:41:04.312997",
    "current_status_date": "2024-10-10 19:41:04.312977",
    "currency": "NGN",
    "session_id": "8c35b4b0-161f-4f94-900b-05c358461d13",
    "merchant_transaction_reference": "Ref1234567890",
    "transaction_type": "VirtualAccount",
    "virtual_account_number": "7000009348",
    "status_reason": "Transfer Successful"
  },
  "success": true
}
```

### Authorization Header Values 

| Header          | Description                               |
| --------------- | ----------------------------------------- |
| `Authorization` | Payaza Base 64 encoded merchant's API key |
| `X-TenantID`    | `test` (Connection mode: live or test)    |
| `X-ProductID`   | `app`                                     |

```json
{
  "Authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTBNzdC",
  "X-TenantID": "test",
  "X-ProductID": "app"
}
```

# XOF Test Account Funding

This endpoint funds test accounts. **Note:** This API is only for testing purposes.

---

## Arguments

| Argument | Type | Description |
|----------|------|-------------|
| `transaction_reference` | string | Unique identifier of the transaction |
| `country_code` | string | ISO Country Code (ISO 3166-1 alpha-2) |

---

## POST Endpoint: https://api.payaza.africa/live/subsidiary/funding/v1/process-collection

### Example Request

```java

OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("application/json");
RequestBody body = RequestBody.create(mediaType, "{
    "transaction_reference": "MOCIV0009384",
    "country_code": "CI"
}");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/subsidiary/funding/v1/process-collection")
  .method("POST", body)
  .addHeader("X-TenantID", "test")
  .addHeader("X-ProductID", "app")
  .addHeader("Content-Type", "application/json")
  .addHeader("Authorization", "Payaza UFo3OC1QS1RFU1QtNzc2RjBBMzktQ0Q4My00RENDLThCRkItRTgxNDdEMEE2MzVG")
  .build();
Response response = client.newCall(request).execute();
```

### Authorization Header Values

| Header          | Description                               |
| --------------- | ----------------------------------------- |
| `Authorization` | Payaza Base 64 encoded merchant's API key |
| `X-TenantID`    | `test` (Connection mode: live or test)    |
| `X-ProductID`   | `app`                                     |

```json
{
  "Authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTBNzdC",
  "X-TenantID": "test",
  "X-ProductID": "app"
}

```


# Libraries

# Payment Page

Welcome to the Payment Page documentation. This guide will help you integrate Payaza’s payment gateway and implement a secure, seamless checkout experience on your website.

---

## Introduction

Payment Page provides a **secure and PCI-compliant solution** for processing online payments. Customers complete their transactions on a secure page hosted by Payaza, reducing your PCI scope and ensuring a smooth checkout experience.

---

## Getting Started

### Retrieving Your API Keys

To use Payment Page Checkout, you need an API key:

1. Log in to your Payaza dashboard.
2. Click on **Settings** in the navigation.
3. Click on **API Key and Webhooks**.

---

## Integration Steps

- Payment Page Checkout URL - https://business.payaza.africa/payment-page Redirects the customer to the Payment Page Checkout page, appending the following parameters to the URL provided above.

- On the Payment Page Checkout page, the customer will enter their payment details and complete the transaction

- Sample URL:
**https://business.payaza.africa/payment-page/?merchant_key=PZ78-PKTEST-9A4086C1-UEHSHS9R&connection_mode=Test&checkout_amount=20¤cy_code=NGN&email_address=rayphil@gmail.com
&first_name=Ray&last_name=Phil&phone_number=08012345678&transaction_reference=b343aseasd
&additional_details={"user_id": 1273,"ticket": "TEUBD9382892"}&redirect_url=https://www.google.com.**

```
html
<!DOCTYPE html>
<html>
<head>
  <title>Sample Webpage</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      background-color: #f5f5f5;
      margin: 0;
      padding: 0;
    }
    
    .container {
      max-width: 800px;
      margin: 0 auto;
      padding: 20px;
      background-color: #ffffff;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }
    
    h1 {
      color: #333333;
      text-align: center;
    }
    
    form {
      max-width: 400px;
      margin: 20px auto;
    }
    
    label {
      display: block;
      font-weight: bold;
      margin-bottom: 5px;
    }
    
    input[type="text"],
    input[type="number"],
    input[type="email"],
    input[type="tel"] {
      width: 100%;
      padding: 10px;
      margin-bottom: 10px;
      border: 1px solid #dddddd;
      border-radius: 3px;
    }
    
    select {
      width: 100%;
      padding: 10px;
      margin-bottom: 10px;
      border: 1px solid #dddddd;
      border-radius: 3px;
    }
    
    input[type="submit"] {
      width: 100%;
      padding: 10px;
      background-color: #4caf50;
      color: #ffffff;
      border: none;
      border-radius: 3px;
      cursor: pointer;
    }
    
    input[type="submit"]:hover {
      background-color: #45a049;
    }
  </style>
</head>
<body>
  <div class="container">
    <h1>Sample Webpage</h1>
    
    <form action="process_form.php" method="POST" id="myForm">
      <label for="merchant_key">Merchant Key:</label>
      <input type="text" id="merchant_key" name="merchant_key" required>

      <label for="connection_mode">Connection Mode:</label>
      <select id="connection_mode" name="connection_mode">
        <option value="Live">Live</option>
        <option value="Test">Test</option>
      </select>

      <label for="checkout_amount">Amount:</label>
      <input type="number" id="checkout_amount" name="checkout_amount" required>

      <label for="currency_code">Currency Code:</label>
      <select id="currency_code" name="currency_code">
        <option value="NGN">NGN</option>
        <option value="USD">USD</option>
      </select>
      
      <label for="email_address">Email:</label>
      <input type="email" id="email_address" name="email_address" required>
      
      <label for="first_name">First Name:</label>
      <input type="text" id="first_name" name="first_name" required>

      <label for="last_name">Last Name:</label>
      <input type="text" id="last_name" name="last_name" required>

      <label for="phone_number">Phone:</label>
      <input type="tel" id="phone_number" name="phone" required>
      
      
      <label for="transaction_reference">Reference:</label>
      <input type="text" id="transaction_reference" name="transaction_reference" required>

      <!-- These are to be sent in the addition_details in json format. They are specific to your use case if you have any extra parameter you want to see on your dashboard -->
      <!-- This is an example for an Airline -->
     
      <label for="pnr">PNR:</label>
      <input type="text" id="pnr" name="Passenger Name Record" required>

      <label for="ticket_number">Ticket Number:</label>
      <input type="text" id="ticket_number" name="Ticket Number" required>

                 
      <label for="redirect_url">Redirect URL:</label>
      <input type="text" id="redirect_url" name="redirect_url" required>

      <input type="submit" value="Submit">
    </form>

  <script>
    document.getElementById("myForm").addEventListener("submit", function(event) {
      event.preventDefault(); // Prevent form submission
      
      // Get form field values
      var merchant_key= document.getElementById("merchant_key").value;
      var connection_mode= document.getElementById("connection_mode").value;
      var checkout_amount = document.getElementById("checkout_amount").value;
      var currency_code = document.getElementById("currency_code").value;
      var email_address = document.getElementById("email_address").value;
      var first_name = document.getElementById("first_name").value;
      var last_name = document.getElementById("last_name").value;
      var phone_number = document.getElementById("phone_number").value;
      var transaction_reference = document.getElementById("transaction_reference").value;

      // Note that these are to be passed as optional parameters
      var pnr = document.getElementById("pnr").value;
      var ticket_number = document.getElementById("ticket_number").value;
      
      // compose the additional details as a JSON object
      var additional_details = JSON.stringify({
        "pnr": pnr,
        "ticket_number" : ticket_number
        });

      var redirect_url = document.getElementById("redirect_url").value;
      
      // Build URL with form field values
      var url = "https://business.payaza.africa/payment-page?merchant_key=" + encodeURIComponent(merchant_key) +
                "&connection_mode=" + encodeURIComponent(connection_mode) +
                "&checkout_amount=" + encodeURIComponent(checkout_amount) +
                "&currency_code=" + encodeURIComponent(currency_code) +
                "&email_address=" + encodeURIComponent(email_address) +
                "&first_name=" + encodeURIComponent(first_name) +
                "&last_name=" + encodeURIComponent(last_name) +
                "&phone_number=" + encodeURIComponent(phone_number) +
                "&transaction_reference=" + encodeURIComponent(transaction_reference) + 
                "&additional_details=" + encodeURIComponent(additional_details) + // Encoded JSON; Note that this is optional 
                "&redirect_url=" + encodeURIComponent(redirect_url);
        const a = document.createElement('a')
        a.href = url
        a.click()
      
      // Redirect to the constructed URL
      // window.location.href = "http://127.0.0.1:5500/Webpage.html";
    });
  </script>

  </div>
</body>
</html>
```

### Arguments

| Argument                | Type   | Description                                          |
| ----------------------- | ------ | ---------------------------------------------------- |
| `merchant_key`          | string | Your Public API key                                  |
| `connection_mode`       | string | `Live` or `Test`                                     |
| `checkout_amount`       | double | Amount to charge the customer                        |
| `currency_code`         | string | Currency to charge in                                |
| `email_address`         | string | Customer email                                       |
| `first_name`            | string | Customer first name                                  |
| `last_name`             | string | Customer last name                                   |
| `phone_number`          | int    | Customer phone number                                |
| `transaction_reference` | string | Unique transaction reference                         |
| `additional_details`    | JSON   | Optional custom payload data (encoded)               |
| `redirect_url`          | string | URL to redirect the customer after payment (encoded) |


### Payment Options
Our Payment Page supports a wide range of payment options, including debit cards, and alternative payment methods.

### Security
We take security seriously and ensure that our Payment Page is compliant with the latest PCI-DSS standards. Our payment gateway encrypts sensitive data and employs robust security measures to protect your customers' payment information.


# Payaza 2.0 WordPress

## Introduction

Merchants can easily integrate our WordPress plugin into their respective WordPress websites to process payments through our checkout.

Please adhere to the steps outlined below to connect to the Payaza plugin:

1. Access your store's WordPress Admin dashboard and navigate to the 'Plugins' section in the left-hand side menu.  
2. Click on the dropdown menu and select the 'Add new' button.  
3. Search for Payaza and install the plugin as directed.  
 <img width="829" height="369" alt="Screenshot 2025-11-12 at 14 37 03" src="https://gist.github.com/user-attachments/assets/ea94e61b-4b50-4cf5-ba4a-a0ef4e4a1806" />

 
4. After installation, locate the 'Installed Plugins' section in the left-hand side menu under Plugins. Activate the Payaza plugin that was recently installed. 
 
 <img width="847" height="412" alt="Screenshot 2025-11-12 at 14 38 18" src="https://gist.github.com/user-attachments/assets/cee2eb6a-8276-4426-b213-ad9aa9f4a32a" />

 
5. Further customization can be done by clicking on Woocommerce > Settings in the left menu. 

<img width="787" height="372" alt="Screenshot 2025-11-12 at 14 39 10" src="https://gist.github.com/user-attachments/assets/5e4f780b-ac55-4c60-a32a-0fb83c5902a6" />


6. Navigate to payments, and click on 'Manage' to configure your settings as required.

<img width="802" height="343" alt="Screenshot 2025-11-12 at 14 39 35" src="https://gist.github.com/user-attachments/assets/a456f992-f55b-4e04-be92-9e1b234b6786" />


# Web SDK
Payaza's Web SDK is used for integrating our Checkout SDK into merchants' websites to receive payments for various goods and services. Merchants can integrate our Web Checkout SDK through the following steps.

### Usage
#### Using CDN
Add the cdn in the head of your html document

```
 <script defer src="https://checkout-v2.payaza.africa/js/v1/bundle.js"></script>
 ```

#### Using npm or yarn

```
npm install payaza-web-sdk
```

or

```
yarn add payaza-web-sdk
```

#### When using CDN
Use the PayazaCheckout.setup(options: object) to initialize your class and the method showPopup() to show the popup

```

<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Document</title>
    <script defer src="https://checkout-v2.payaza.africa/js/v1/bundle.js"></script>

    <script defer>

        function handleButtonClick() {
            const payazaCheckout = PayazaCheckout.setup({ 
                merchant_key:  "PZ78-PKTEST-B0F603C4-7787-432D-B105-C7FEEDF472E1",
                connection_mode: "Test", // Live || Test
                checkout_amount: Number(10),
                // country_code: "BEN", //either BEN or CIV. This parameter is for CIV/BEN Collections
                currency_code: "NGN",   
                email_address: "johndoe@gmail.com",
                first_name: 'Big',
                last_name: 'Maitre',
                phone_number: "01232425262",
                transaction_reference: "PL" + Math.floor(
                    (Math.random() * 10000000) + 1
                ),
                //Set Virtual account time limit (optional)
                virtual_account_configuration: {
                    "expires_in_minutes": 15
                }, 
                
                  // Additional Details (metadata)
                additional_details: {
                    user_id: "1273",
                    ticket: "TEUBD9382892"
                }               
            });

            // You can set the onClose and callback function as described below
            function callback(callbackResponse) {
                console.log('callbackResponse: ', callbackResponse)
            }

            function onClose() {
                console.log("closed")
                window.location.href = 'https://google.com'
            }

            payazaCheckout.setCallback(callback)
            payazaCheckout.setOnClose(onClose)

            // Display popup
            payazaCheckout.showPopup();
        }//end function handleButtonClick

    </script>

</head>

<body>
<button onclick="handleButtonClick()">Proceed!!!</button>
</body>
```

#### When using npm or yarn
You can use the sdk any of the following ways

```
import PayazaCheckout from "payaza-web-sdk";
...
const payazaCheckout = new PayazaCheckout({
  merchant_key: "<public key>",
  connection_mode: "Live", // Live || Test
  checkout_amount: Number(2000),
  currency_code: "NGN",   
  email_address: "example@email.com",
  first_name: '<first name>',
  last_name: '<last name>',
  phone_number: "+1200000000",
  transaction_reference: 'your_reference',
  //Set Virtual account time limit (optional)
  virtual_account_configuration: {
  "expires_in_minutes": 15
  }, 
  
  //Additional Details (metadata)
   additional_details: {
      user_id: "1273",
      ticket: "TEUBD9382892"
  },
  
  onClose: function() {
    console.log("Closed")
  },
  callback: function(callbackResponse) {
    console.log(callbackResponse)
  }
});

// Alternatively, you can set the onClose and callback function as described below
function callback(callbackResponse){
  console.log(callbackResponse)
}

function onClose(){
  console.log("closed")
}

payazaCheckout.setCallback(callback)
payazaCheckout.setOnClose(onClose)

// Display popup
payazaCheckout.showPopup();
// Display popup
payazaCheckout.showPopup();
```

```
import {setup} from "payaza-web-sdk";
...
const payazaCheckout = setup({});
payazaCheckout.showPopup();
```

and if you are using typescript

```typescript

<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Document</title>
    <script defer src="https://checkout-v2.payaza.africa/js/v1/bundle.js"></script>

    <script defer>

        function handleButtonClick() {
            const payazaCheckout = PayazaCheckout.setup({ 
                merchant_key:  "PZ78-PKTEST-B0F603C4-7787-432D-B105-C7FEEDF472E1",
                connection_mode: "Test", // Live || Test
                checkout_amount: Number(10),
                // country_code: "BEN", //either BEN or CIV. This parameter is for CIV/BEN Collections
                currency_code: "NGN",   
                email_address: "johndoe@gmail.com",
                first_name: 'Big',
                last_name: 'Maitre',
                phone_number: "01232425262",
                transaction_reference: "PL" + Math.floor(
                    (Math.random() * 10000000) + 1
                ),
                //Set Virtual account time limit (optional)
                virtual_account_configuration: {
                    "expires_in_minutes": 15
                }, 
                
                  // Additional Details (metadata)
                additional_details: {
                    user_id: "1273",
                    ticket: "TEUBD9382892"
                }               
            });

            // You can set the onClose and callback function as described below
            function callback(callbackResponse) {
                console.log('callbackResponse: ', callbackResponse)
            }

            function onClose() {
                console.log("closed")
                window.location.href = 'https://google.com'
            }

            payazaCheckout.setCallback(callback)
            payazaCheckout.setOnClose(onClose)

            // Display popup
            payazaCheckout.showPopup();
        }//end function handleButtonClick

    </script>

</head>

<body>
<button onclick="handleButtonClick()">Proceed!!!</button>
</body>
```

if the setup function conflict with one of your functions, you can rename it


```
import {setup as PayazaSetup} from "payaza-web-sdk";
...
const payazaCheckout = PayazaSetup({});
payazaCheckout.showPopup();
```

#### Callback
The callback function is an event hook through which Payaza sends data.

#### Callback Response object

```json
"type": "success",
    "status": 201,
    "data": {
        "message": "Transaction Successful",
        "payaza_reference": "P-C-20231018-1TLB7K68",
        "transaction_reference": "PL296969",
        "transaction_fee": 100,
        "transaction_total_amount": 105,
        "currency": {
            "name": "Naira",
            "code": "NGN",
            "unicode": "₦",
            "html_value": "&#8358;"
        },
        "customer": {
            "customer_id": "HCC4ZX96W",
            "email_address": "johndoe@gmail.com",
            "first_name": "Big",
            "last_name": "Maitre",
            "mobile_number": "01232425262"
        }
    }
}
```

### Errors
To avoid any error, ensure that all requested parameters are provided in the right format before initiating the transaction.

```json
{
    "type": "error",
    "status": 401,
    "data": {
        "message": "Sorry merchant key is not valid"
    }
}
```


```json
Validation error

{
    "type": "error",
    "status": 400,
    "data": {
        "message": "Error during validation",
        "errors": [
            {
                "field": "merchant_key",
                "errors": [
                    "'merchant_key' is required"
                ]
            },
            {
                "field": "checkout_amount",
                "errors": [
                    "'checkout_amount' must be numeric"
                ]
            },
            {
                "field": "first_name",
                "errors": [
                    "'first_name' cannot be blank"
                ]
            },
            {
                "field": "email_address",
                "errors": [
                    "'email_address' cannot be blank",
                    "'email_address' must be a valid email address"
                ]
            }
        ]
    }
}
```

```json
Connection code mismatch

{
    "type": "error",
    "status": 401,
    "data": {
        "message": "Business Profile Credentials does not match connection mode selected"
    }
}
```

## Arguments

- **merchant_key** `string`  
  This is your public key.

- **connection_mode** `string`  
  Mode of session (Test or Live)

- **checkout_amount** `double`  
  Amount to charge the customer

- **country_code** `string`  
  It refers to the code of the country e.g. BEN, CIV etc. This is used for CIV/BEN Collections

- **currency_code** `string`  
  It refers to the code of the currency (NGN, USD etc.)

- **email_address** `string`  
  The email address of the customer

- **first_name** `string`  
  The first name of the customer

- **last_name** `string`  
  The last name of the customer

- **phone_number** `int`  
  The phone number of the customer

- **transaction_reference** `string`  
  The unique identifier given to a particular transaction by the merchant.

- **payaza_reference** `string`  
  This is Payaza's transaction reference that is returned in the callback response for the transaction. This is unique and no 2 transactions can have the same reference.

- **additional_details** `JSON`  
  Custom data to your payload



# Check Transaction Status (Merchant Reference)

## Introduction
This endpoint is used to check the transaction status of a transaction using the Merchant Reference.

## Arguments

- **merchant_reference** `string`  
  The reference generated to the transaction that is placed in the Checkout or Payment page request

## GET Endpoint: https://api.payaza.africa/live/merchant-collection/transfer_notification_controller/merchant/transaction-query?merchant_reference={{merchantreference}}

## Example Request

```java

  OkHttpClient client = new OkHttpClient().newBuilder()
  .build();
MediaType mediaType = MediaType.parse("text/plain");
RequestBody body = RequestBody.create(mediaType, "");
Request request = new Request.Builder()
  .url("https://api.payaza.africa/live/merchant-collection/transfer_notification_controller/merchant/transaction-query?merchant_reference=laxpre")
  .method("GET", body)
  .addHeader("Authorization", "Payaza UFo3OC1QS1CFU1QEN0IyMpNGNTUtQzVCQS00QzU4LTkwNQYtRDI5RTJCNTVFNjc4")
  .build();
Response response = client.newCall(request).execute();
```
## Sample Response

```json
{
  "message": "Transaction data found",
  "data": {
    "transaction_reference": null,
    "amount_received": 20.28,
    "transaction_fee": 0.28,
    "transaction_status": "Completed",
    "sender_name": "Ray Phil",
    "sender_account_number": null,
    "source_bank_name": "MASTERCARD",
    "initiated_date": "2023-02-11 09:49:09.254",
    "current_status_date": "2023-02-11 09:49:09.254",
    "currency": "NGN",
    "session_id": "419309171231",
    "merchant_transaction_reference": "3bf267326",
    "transaction_type": "Card",
    "virtual_account_number": null,
    "status_reason": "Payment Approved"
  },
  "success": true
}
```

### Authorization Header Values

---

Authorization string
Payaza Base 64 encoded merchant's API key

---

x-TenantID string
Connection mode: “live” or “test”

---

```json
{ 
 "authorization": "Payaza UFo3OC1QS0xJVkUtRjMwODcwNUMtRkY2NC00MEJCLTg1OUUtM0ZCQUI4MTJBNzdC"
}
```

# Mobile SDKs

# iOS Swift SDK

## Introduction
iOS SDK aids in processing payment through the following channels: Cards, Bank, and Virtual Transfer.

## Example
To run the example project, clone the repo, and run `pod install` from the Example directory first.

## Requirements
- Payaza SDK is compatible with iOS apps running on iOS 11.0 and above.
- Requires Xcode 10.0+ to build the source.

## Installation
PayAzaSDK is available through CocoaPods. To install it, add the following line to your Podfile:

```
source 'https://github.com/78-Financials/Specs.git'
source 'https://github.com/CocoaPods/Specs.git'
pod 'PayAzaSDK'
```

In your terminal, run

```
  pod update && pod install

```

## Usage
There are three steps you would have to complete to set up the SDK and perform transaction

Install the SDK as a dependency
Configure the SDK with Merchant Information
Initiate payment with customer details

```Swift
Request Sample



import PayazaPod

class ViewController: UIViewController, PayazaCallbackMethods {

private var transactionAmount: Int64?



@objc func showExample(){
  let baseUrl = "https://your-base-url"
  let manager = PayazaManager()
  transactionAmount = 100
  manager.initialize(delegateController: self, viewControler: self)
  manager.PayAzaConfig(merchantKey: "Your Merchant Key Here", merchantName: "Test Merchant", currency: "NGN", firstname: "Firstname",
  lastname: "Lastname", email: "emailAdreess", phone: "Phone", transactionRef: "transactionReference", 
  amount: transactionAmount!, isLive: true, baseUrl: baseUrl)  // Set isLive to false during testing and set to true during production
  manager.payNow()
}
 func onPaymentComplete(response: TransactionResponse) {
       print("Successful with (response.reference ?? "Failed to return data")")
 }

func onPaymentCancelled(errorMessage: String) {
   print( errorMessage)
}
}
```

### Request Parameters / Arguments

---

baseUrl string
The URL of the payment service

---

email string
The email address of the customer sending the money

---

firstname string
The first name of the customer sending the money

---

lastname string
The last name of the customer sending the money

---

phone string
Phone number of the customer sending the money. Must be in international standard (e.g., +2348012345678)

---

transactionRef string
The transaction reference generated for the transaction. Unique; no two transactions can have the same reference

---

merchantKey string
Your public key

---

isLive boolean
Mode of session (False = test mode, True = live mode)

---

transactionAmount double
The amount the customer is expected to pay

---

currency string
The code of the currency (NGN, USD, etc.)


# React Native

## Payaza React Native Library

### Installation

```bash
npm install react-native-payaza
```
or

```
yarn add react-native-payaza
```

## Installing Dependencies
### Install React Native Webview

```
npm install react-native-webview
```

### For Bear React Native flow

```
npm install --save @react-native-clipboard/clipboard
```

### Link native dependecies

From react-native 0.60 autolinking will take care of the link step but don't forget to run pod install
React Native modules that include native Objective-C, Swift, Java, or Kotlin code have to be "linked" so that the compiler knows to include them in the app.

```
react-native link react-native-webview
```

## Usage
Import the package

```
import Payaza, {
  type IPayaza,
  type PayazaErrorResponse,
  type PayazaSuccessResponse,
  PayazaConnectionMode,
} from 'react-native-payaza'
// ...
const payaza = React.useRef<IPayaza>(null);
// ...
const payNow = () => {
  payaza.current?.createTransaction({
    amount: Number(110),
    connectionMode: PayazaConnectionMode.LIVE_CONNECTION_MODE,
    email: "example@example.com",
    firstName: "<first name>",
    lastName: "<last name>",
    phoneNumber: "<+12345678900>",
    currencyCode: 'NGN',
    transactionReference: "transaction_reference",
  });
}
  const handleError = (response: PayazaErrorResponse) => {
    Alert.alert(response.data.message, 'Error Occurred');
  };
  const handleSuccess = (response: PayazaSuccessResponse) => {
    Alert.alert(
      response.data.message,
      `Transaction reference {$response.data.payaza_reference}`
    );
  };
// ...
return (
  <View>
    <TouchableOpacity onPress={payNow}>
      <Text style={styles.buttonText}>Pay Now</Text>
    </TouchableOpacity>
    <Payaza
      onSuccess={handleSuccess}
      onError={handleError}
      onClose={console.log}
      merchantKey="<public key>"
      ref={payaza}
    />
  </View>
)

```


## Arguments

---

email string

The email address of the customer sending the money

---

firstName string

The first name of the customer sending the money

---

lastName string

The last name of the customer sending the money

---

phoneNumber string

Phone number of the customer sending the money. It must be international standard i.e. +2348012345678

---

transactionReference string

This is the transaction reference that you generated for the transaction. This unique and no 2 transactions can have the same reference.

---

merchantKey string

This is your public key.

---

connectionMode string

Mode of session (LIVE_CONNECTION_MODE or TEST_CONNECTION_MODE )

---

amount double

The amount the customer is expected to pay.

---

currencyCode string

It refers to the code of the currency.(NGN, USD etc.)

---


# Flutter

Payaza’s Flutter SDK makes it easy for you to start accepting payments from your customers when they visit your applications. The checkout SDK can be integrated in very easy steps, making it the easiest way to start accepting payments.

## Getting started
To start collecting payment with Payaza install payaza widget by adding as a in pubspec.yaml dependency

```
dependencies:
    payaza: ^1.0.0
```

## Usage

Initialize SDK anywhere before use

```
  //...
  Payaza.init('<public key>');
  runApp(const MyApp());
  //...
```

```
    // ...
    import 'package:payaza/payaza.dart';
    // ...

    void handleSuccess(PayazaSuccessResponse response) async {
        await showAlert(
            message: response.data.payazaReference ?? '',
            title: 'Payment Successful');
        if (context.mounted) {
        Navigator.of(context).pop();
        }
    }

  void handleError(PayazaErrorResponse response) async {
    await showAlert(message: response.data.message, title: 'Error');
    if (context.mounted) {
      Navigator.of(context).pop();
    }
  }

  void handleClose() {
    print('Payaza widget was closed');
  }

  void onSubmit() {
    Payaza.createTransaction(
      context,
      config: PayazaConfig(
        amount: 110,
        connectionMode: PayazaConnectionMode.LIVE_CONNECTION_MODE,
        email: "example@example.com",
        firstName: "<first name>",
        lastName: "<last name>",
        phoneNumber: "<+12345678900>",
        transactionReference: "transaction_reference",
        currencyCode: <NGN>,
      ),
      onSuccess: handleSuccess,
      onError: handleError,
      onClose: handleClose,
    );
  }

```


### Arguments

---

email string

The email address of the customer sending the money

---

firstName string

The first name of the customer sending the money

---

lastName string

The last name of the customer sending the money

---

phoneNumber string

Phone number of the customer sending the money. It must be international standard i.e. +2348012345678

---

transactionReference string

This is the transaction reference that you generated for the transaction. This unique and no 2 transactions can have the same reference.

---

connectionMode string

Mode of session (LIVE_CONNECTION_MODE or TEST_CONNECTION_MODE )

---

amount double

The amount the customer is expected to pay.

---

currencyCode string

It refers to the code of the currency(NGN, USD etc.). It defaults to NGN if parameter is not passed

---