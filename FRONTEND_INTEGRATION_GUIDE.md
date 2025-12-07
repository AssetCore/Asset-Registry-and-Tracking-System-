# Frontend Integration Guide

## 🚀 Quick Start

### Your Running Services
- **Asset Registry**: http://localhost:5001
- **Identity & Access**: http://localhost:5003  
- **Maintenance Scheduler**: http://localhost:5004
- **Notification**: http://localhost:5005

---

## 📡 API Endpoints Reference

### 1. Asset Registry Service (Port 5001)

#### Assets Management
```javascript
// Create Asset
POST http://localhost:5001/api/assets
Body: {
  "name": "Laptop HP 15",
  "serialNumber": "SN12345",
  "categoryId": "guid",
  "locationId": "guid",
  "purchaseDate": "2024-01-15",
  "purchasePrice": 1200.00,
  "status": "Active"
}

// Get All Assets (Paginated)
GET http://localhost:5001/api/assets?page=1&pageSize=50

// Get Asset by ID
GET http://localhost:5001/api/assets/{id}

// Update Asset
PUT http://localhost:5001/api/assets/{id}
Body: { ...asset data }

// Delete/Deactivate Asset
DELETE http://localhost:5001/api/assets/{id}

// Restore Asset
POST http://localhost:5001/api/assets/{id}/restore
```

#### Categories & Locations
```javascript
// Get Categories
GET http://localhost:5001/api/categories

// Get Locations
GET http://localhost:5001/api/locations
```

---

### 2. Identity & Access Service (Port 5003)

#### User Management
```javascript
// Get All Users
GET http://localhost:5003/api/Users

// Get User by ID
GET http://localhost:5003/api/Users/{id}

// Create User
POST http://localhost:5003/api/Users
Body: {
  "userName": "john.doe",
  "name": {
    "givenName": "John",
    "familyName": "Doe"
  },
  "emails": [{
    "value": "john@example.com",
    "primary": true
  }]
}

// Update User
PUT http://localhost:5003/api/Users/{id}

// Delete User
DELETE http://localhost:5003/api/Users/{id}
```

#### Roles
```javascript
// Get Roles
GET http://localhost:5003/api/Role
```

#### Current User
```javascript
// Get Current User Info
GET http://localhost:5003/api/Me
```

---

### 3. Maintenance Scheduler Service (Port 5004)

#### Maintenance Schedules
```javascript
// Get All Schedules
GET http://localhost:5004/api/MaintenanceSchedule

// Get Schedule by ID
GET http://localhost:5004/api/MaintenanceSchedule/{id}

// Get Schedules for Specific Asset
GET http://localhost:5004/api/MaintenanceSchedule/asset/{assetId}

// Get Upcoming Maintenance (next 30 days)
GET http://localhost:5004/api/MaintenanceSchedule/upcoming?days=30

// Create Schedule
POST http://localhost:5004/api/MaintenanceSchedule
Body: {
  "assetId": "guid",
  "scheduledDate": "2024-02-01",
  "maintenanceType": "Preventive",
  "description": "Regular maintenance check",
  "estimatedCost": 150.00
}

// Update Schedule
PUT http://localhost:5004/api/MaintenanceSchedule/{id}

// Delete Schedule
DELETE http://localhost:5004/api/MaintenanceSchedule/{id}
```

#### Warranty Info
```javascript
// Get Warranty Information
GET http://localhost:5004/api/WarrantyInfo
GET http://localhost:5004/api/WarrantyInfo/{id}
```

#### Maintenance History
```javascript
// Get Maintenance History
GET http://localhost:5004/api/MaintenanceHistory
GET http://localhost:5004/api/MaintenanceHistory/asset/{assetId}
```

---

### 4. Notification Service (Port 5005)

#### Send Notifications
```javascript
// Send Notification
POST http://localhost:5005/api/Notification/send
Body: {
  "type": "MaintenanceDue", // or "AssetUpdate", "WarrantyExpiring"
  "channel": "Email", // or "Slack"
  "emailAddress": "user@example.com",
  "recipientName": "John Doe",
  "subject": "Maintenance Due",
  "body": "Your asset maintenance is due on...",
  "assetId": "guid",
  "assetName": "Laptop HP 15"
}

// Get Notification by ID
GET http://localhost:5005/api/Notification/{id}

// Get User Notifications
GET http://localhost:5005/api/Notification/user/{userId}

// Mark as Read
PUT http://localhost:5005/api/Notification/{id}/read
```

---

## 🔧 Frontend Implementation Examples

### React/Vue/Angular - Using Fetch API

#### 1. Create Asset
```javascript
async function createAsset(assetData) {
  try {
    const response = await fetch('http://localhost:5001/api/assets', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(assetData)
    });
    
    if (!response.ok) throw new Error('Failed to create asset');
    
    const result = await response.json();
    return result;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}

// Usage
const newAsset = {
  name: "Dell Monitor",
  serialNumber: "MON-12345",
  categoryId: "some-guid",
  locationId: "some-guid",
  purchaseDate: "2024-01-15",
  purchasePrice: 300.00
};

createAsset(newAsset);
```

#### 2. Get All Assets
```javascript
async function getAllAssets(page = 1, pageSize = 50) {
  try {
    const response = await fetch(
      `http://localhost:5001/api/assets?page=${page}&pageSize=${pageSize}`
    );
    
    if (!response.ok) throw new Error('Failed to fetch assets');
    
    const assets = await response.json();
    return assets;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}

// Usage
getAllAssets(1, 20).then(assets => {
  console.log('Assets:', assets);
});
```

#### 3. Update Asset
```javascript
async function updateAsset(assetId, updatedData) {
  try {
    const response = await fetch(`http://localhost:5001/api/assets/${assetId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(updatedData)
    });
    
    if (!response.ok) throw new Error('Failed to update asset');
    
    return true;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}
```

#### 4. Get Upcoming Maintenance
```javascript
async function getUpcomingMaintenance(days = 30) {
  try {
    const response = await fetch(
      `http://localhost:5004/api/MaintenanceSchedule/upcoming?days=${days}`
    );
    
    if (!response.ok) throw new Error('Failed to fetch maintenance schedules');
    
    const schedules = await response.json();
    return schedules;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}
```

#### 5. Send Notification
```javascript
async function sendNotification(notificationData) {
  try {
    const response = await fetch('http://localhost:5005/api/Notification/send', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(notificationData)
    });
    
    if (!response.ok) throw new Error('Failed to send notification');
    
    const result = await response.json();
    return result;
  } catch (error) {
    console.error('Error:', error);
    throw error;
  }
}

// Usage
sendNotification({
  type: "MaintenanceDue",
  channel: "Email",
  emailAddress: "user@example.com",
  recipientName: "John Doe",
  subject: "Maintenance Alert",
  body: "Your asset requires maintenance",
  assetId: "some-guid",
  assetName: "Laptop HP 15"
});
```

---

### Using Axios (Alternative)

```javascript
import axios from 'axios';

// Create an axios instance
const apiClient = axios.create({
  baseURL: 'http://localhost:5001', // Change per service
  headers: {
    'Content-Type': 'application/json'
  }
});

// Asset Service
export const assetService = {
  getAll: (page = 1, pageSize = 50) => 
    apiClient.get(`/api/assets?page=${page}&pageSize=${pageSize}`),
  
  getById: (id) => 
    apiClient.get(`/api/assets/${id}`),
  
  create: (assetData) => 
    apiClient.post('/api/assets', assetData),
  
  update: (id, assetData) => 
    apiClient.put(`/api/assets/${id}`, assetData),
  
  delete: (id) => 
    apiClient.delete(`/api/assets/${id}`),
};

// Usage
assetService.getAll(1, 20)
  .then(response => console.log(response.data))
  .catch(error => console.error(error));
```

---

## 🔐 Important: CORS Configuration

⚠️ **ISSUE**: Your backend services need CORS enabled to accept requests from your frontend.

### Quick Test
Open browser console on your frontend and try:
```javascript
fetch('http://localhost:5001/api/assets')
  .then(r => r.json())
  .then(d => console.log(d));
```

If you see a CORS error, I need to add CORS configuration to your services.

**Would you like me to add CORS configuration to all services now?**

---

## 🧪 Testing Your APIs

### Using Browser
1. **Swagger UI** (if available):
   - Asset Registry: http://localhost:5001/swagger
   - Identity Access: http://localhost:5003/swagger
   - Maintenance: http://localhost:5004/swagger
   - Notification: http://localhost:5005/swagger

### Using Postman or Thunder Client
Import these endpoints and test each service individually before frontend integration.

### Using cURL
```bash
# Get all assets
curl http://localhost:5001/api/assets

# Create asset
curl -X POST http://localhost:5001/api/assets \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Asset","serialNumber":"TEST123"}'
```

---

## 📦 Recommended Frontend Structure

```
src/
├── services/
│   ├── assetService.js        # Asset Registry API calls
│   ├── userService.js         # Identity/Access API calls
│   ├── maintenanceService.js  # Maintenance API calls
│   └── notificationService.js # Notification API calls
├── components/
│   ├── Assets/
│   ├── Users/
│   ├── Maintenance/
│   └── Notifications/
└── utils/
    └── apiClient.js           # Axios/Fetch configuration
```

---

## 🚨 Common Issues & Solutions

### 1. CORS Errors
**Solution**: Add CORS middleware to backend services (I can help with this)

### 2. Connection Refused
**Solution**: Verify containers are running: `docker-compose ps`

### 3. 404 Not Found
**Solution**: Check endpoint paths match exactly (case-sensitive)

### 4. 500 Internal Server Error
**Solution**: Check backend logs: `docker-compose logs [service-name]`

---

## 📝 Next Steps

1. **Enable CORS** on all backend services
2. **Test each endpoint** using Postman/Swagger
3. **Create API service layer** in your frontend
4. **Build UI components** that call these services
5. **Add error handling** and loading states
6. **Implement authentication** if needed

---

## 🆘 Need Help?

Let me know if you need:
- ✅ CORS configuration added to services
- ✅ Sample frontend code for specific features
- ✅ Help with error handling
- ✅ Authentication setup
- ✅ Docker logs analysis
