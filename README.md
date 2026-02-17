# Optimizely Quick Content

A WPF desktop tool for creating content in Optimizely CMS via the Content Management API using OpenID Connect authentication.

---

## Prerequisites

### Optimizely CMS Setup

#### 1. Install the OpenID Connect package

#### 2. Register the Content Management API in `Startup.cs`
```
services.AddContentManagementApi(OpenIDConnectOptionsDefaults.AuthenticationScheme, options => {
   options.DisableScopeValidation = true;
});
services.AddOpenIDConnect<ApplicationUser>(
   useDevelopmentCertificate: true,
   signingCertificate: null,
   encryptionCertificate: null,
   createSchema: true
);
services.AddOpenIDConnectUI();
```

#### 3. Create an OpenID Connect Application

1. Go to **CMS → Settings → OpenID Connect**
2. Click **Create New Application**
3. Select **Confidential**
4. Enter a **Client ID** and **Client Secret**

   > ⚠️ **Important:** The Client Secret will never be visible again after saving. Copy it immediately.

5. Set **Scopes** to `epi_content_management`
6. Click **Save**

#### 4. Set Access Rights

1. Go to **CMS → Settings → Set Access Rights**
2. Select the **Root** node
3. Click **Add User/Group**
4. Select the application you just created
5. Assign the appropriate rights (minimum: **Read** and **Create**)
6. Click **Save Access Rights**

---

## Using the Tool

### 1. Add a Connection

- Click the **＋ New** button
- Enter:
  - **Display Name** — a friendly name for this connection within the tool
  - **Base URL** — your Optimizely CMS base URL (e.g. `https://my-cms.example.com`)
  - **Client ID** — from the OpenID Connect application
  - **Client Secret** — from the OpenID Connect application
- Click **Save Connection**

### 2. Connect

- Select a connection from the dropdown
- Click **⚡ Connect** to retrieve an access token

### 3. Send a Request

- Paste your API request JSON into the text field
- Click **Send Request**

### Example Request Body

```json
{
  "name": "Sample Page",
  "contentType": "Page",
  "language": "en",
  "parentLink": "00000000-0000-0000-0000-000000000000",
  "status": "Published",
  "property": {
    "Title": "This is a sample page",
    "Content": "<p>Hello, world!</p>"
  }
}
```

---

## ⚠️ Important Notes

- **Nested content is not supported** — this tool cannot create content blocks within content.
- Every request **requires** the following properties:
  - `language` — the language code (e.g. `en`)
  - `parentLink` — the parent content ID
  - `status` — the publish status (e.g. `Published`, `CheckedOut`)
