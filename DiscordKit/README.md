# 🤖 Discord Dev Log Kit

A plug-and-play toolkit for posting beautiful embed messages to a Discord channel using webhooks — no coding required.

---

## What's Included

| File | Purpose |
|------|---------|
| `post_devlog.ps1` | PowerShell script that sends formatted embeds to Discord |
| `discord_config.json` | Your personal config (webhook URL, bot name) |
| `README.md` | You're reading it! |

---

## Quick Start (5 minutes)

### Step 1: Create a Discord Webhook

1. Open **Discord** and go to the server where you want to post
2. Right-click the **channel** you want to post to → **Edit Channel**
3. Go to **Integrations** → **Webhooks** → **New Webhook**
4. Give it a name (e.g., `DevLog Bot`) and optionally set an avatar
5. Click **Copy Webhook URL** — you'll need this in the next step

> **💡 Tip:** If you want posts to forward to other servers, the channel needs to be set up as an **Announcement Channel**. Go to **Edit Channel** → **Overview** → enable **Announcement Channel**. Then other servers can **Follow** that channel.

### Step 2: Configure Your Settings

Open `discord_config.json` in any text editor and fill in your values:

```json
{
  "webhook_url": "PASTE_YOUR_WEBHOOK_URL_HERE",
  "bot_name": "My DevLog"
}
```

| Field | What to Put |
|-------|-------------|
| `webhook_url` | The webhook URL you copied in Step 1 |
| `bot_name` | The display name for your bot's messages |

Save the file.

### Step 3: Post Your First Message

Open **PowerShell** in this folder and run:

```powershell
.\post_devlog.ps1 -Title "Hello World!" -Changes "My first dev log post","It works!"
```

You should see a beautiful embed appear in your Discord channel! 🎉

---

## Usage

### Basic Post

```powershell
.\post_devlog.ps1 -Title "Session Title" -Changes "Change 1","Change 2","Change 3"
```

### Custom Color

Colors are decimal integers. Some handy presets:

| Color | Value | Hex |
|-------|-------|-----|
| 🔵 Blue | `3447003` | `#3498DB` |
| 🟢 Green | `3066993` | `#2ECC71` |
| 🟠 Orange | `15105570` | `#E67E22` |
| 🔴 Red | `15158332` | `#E74C3C` |
| 🟡 Gold | `15844367` | `#F1C40F` |
| 🟣 Purple | `10181046` | `#9B59B6` |

```powershell
.\post_devlog.ps1 -Title "Bug Fixes" -Changes "Fixed crash on load","Patched memory leak" -Color 3066993
```

### Custom Footer

```powershell
.\post_devlog.ps1 -Title "v0.2 Release" -Changes "New features","Performance improvements" -Footer "My Game | v0.2.0"
```

---

## Requirements

- **Windows** with **PowerShell 5.1+** (included with Windows 10/11)
- A Discord server where you have **Manage Webhooks** permission

That's it — no installs, no dependencies, no code to compile.

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `webhook_url not found` | Make sure you edited `discord_config.json` and saved it |
| `Invoke-RestMethod` error | Check that your webhook URL is correct and the webhook hasn't been deleted |
| Posts don't forward to other servers | The source channel must be an **Announcement Channel**, and other servers must **Follow** it |
| Script won't run | Run: `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned` |

---

## How Channel Following Works

If you want your posts to automatically appear in other Discord servers:

1. Make your posting channel an **Announcement Channel** (server settings)
2. Other server admins click **Follow** on your channel
3. Every message posted to your announcement channel gets forwarded automatically
4. Webhook messages and embeds forward just like regular messages ✅
