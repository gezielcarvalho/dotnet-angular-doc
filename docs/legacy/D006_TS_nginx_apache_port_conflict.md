# Troubleshooting: Nginx Won't Start - Port Conflict with Apache2

**Date:** November 28, 2025  
**Issue:** Nginx service failed to start, websites not accessible  
**Root Cause:** Apache2 was using port 80, preventing Nginx from binding

---

## Symptoms

- Nginx service shows as `failed` with exit code 1
- Error messages in logs:
  ```
  nginx: [emerg] bind() to 0.0.0.0:80 failed (98: Unknown error)
  nginx: [emerg] bind() to [::]:80 failed (98: Unknown error)
  nginx: [emerg] still could not bind()
  ```
- Websites not accessible (gezielcarvalho.info, moodle sites, etc.)
- `sudo systemctl status nginx` shows "Failed to start"

---

## Diagnosis Steps

### 1. Check Nginx Status

```bash
sudo systemctl status nginx
```

**Output showed:**

```
× nginx.service - A high performance web server and a reverse proxy server
   Active: failed (Result: exit-code)
```

### 2. Verify Nginx Configuration

```bash
sudo nginx -t
```

**Result:** Configuration was valid (syntax OK), so the issue was not config-related.

### 3. Check What's Using Port 80

```bash
sudo netstat -tulpn | grep :80
# OR
sudo ss -tulpn | grep :80
```

**Output revealed:**

```
tcp6  0  0  :::80  :::*  LISTEN  40465/apache2
```

**Finding:** Apache2 was running and occupying port 80, preventing Nginx from starting.

### 4. Check if Site is Enabled

```bash
ls -la /etc/nginx/sites-enabled/ | grep gezielcarvalho
```

**Result:** Sites were properly enabled (symlinks existed).

---

## Root Cause

Apache2 and Nginx were both configured to listen on port 80. When the system booted:

1. Apache2 started first and claimed port 80
2. Nginx tried to start but failed because port 80 was already in use
3. All Nginx-configured sites became inaccessible

This commonly happens when:

- Both web servers are installed on the same system
- Apache2 is set to start automatically (`systemd` enabled)
- No port separation is configured

---

## Solution

Since all websites were configured for Nginx (not Apache2), we disabled Apache2 and started Nginx.

### Step 1: Stop Apache2

```bash
sudo systemctl stop apache2
```

### Step 2: Disable Apache2 from Auto-Starting

```bash
sudo systemctl disable apache2
```

**Output:**

```
Removed /etc/systemd/system/multi-user.target.wants/apache2.service
```

### Step 3: Start Nginx

```bash
sudo systemctl start nginx
```

### Step 4: Verify Nginx is Running

```bash
sudo systemctl status nginx
```

**Expected output:**

```
● nginx.service - A high performance web server and a reverse proxy server
   Active: active (running)
```

### Step 5: Verify Ports are Listening

```bash
sudo netstat -tulpn | grep nginx
```

**Expected output:**

```
tcp  0  0  0.0.0.0:80     0.0.0.0:*  LISTEN  58453/nginx
tcp  0  0  0.0.0.0:443    0.0.0.0:*  LISTEN  58453/nginx
```

### Step 6: Test Websites

```bash
# Test main site
curl -I https://gezielcarvalho.info

# Test subdomains
curl -I https://moodle001.gezielcarvalho.info
curl -I https://wp001.gezielcarvalho.info
```

---

## Alternative Solutions

If you need **both Apache2 and Nginx** running:

### Option 1: Apache2 on Different Port

```bash
# Edit Apache2 ports configuration
sudo nano /etc/apache2/ports.conf

# Change:
Listen 80
# To:
Listen 8081

# Also update virtual hosts
sudo nano /etc/apache2/sites-available/000-default.conf
# Change:
<VirtualHost *:80>
# To:
<VirtualHost *:8081>

# Restart Apache2
sudo systemctl restart apache2
```

Then configure Nginx to reverse proxy to Apache2 when needed:

```nginx
location /apache-app/ {
    proxy_pass http://localhost:8081/;
}
```

### Option 2: Use Only Apache2

If you prefer Apache2, disable Nginx instead:

```bash
sudo systemctl stop nginx
sudo systemctl disable nginx
sudo systemctl start apache2
```

Then migrate all Nginx configurations to Apache2 VirtualHosts.

---

## Prevention

### Check What's Using Ports Before Starting Services

```bash
# Check port 80
sudo lsof -i :80

# Check port 443
sudo lsof -i :443

# List all listening ports
sudo netstat -tulpn | grep LISTEN
```

### Verify Service States

```bash
# Check if Apache2 is enabled
sudo systemctl is-enabled apache2

# Check if Nginx is enabled
sudo systemctl is-enabled nginx
```

### Monitor Port Conflicts

Add to your server monitoring:

```bash
# Create a quick check script
cat > ~/check-webserver.sh << 'EOF'
#!/bin/bash
if ! sudo systemctl is-active --quiet nginx; then
    echo "WARNING: Nginx is not running!"
    sudo netstat -tulpn | grep :80
fi
EOF
chmod +x ~/check-webserver.sh
```

---

## Related Files

All websites affected by this issue:

- `gezielcarvalho.info` - Personal website
- `moodle001.gezielcarvalho.info` - Moodle instance #1
- `moodle002.gezielcarvalho.info` - Moodle instance #2
- `wp001.gezielcarvalho.info` - WordPress site
- `qa-jenkins.sabresoftware.com.br` - Jenkins (infrastructure)
- `qa-portainer.sabresoftware.com.br` - Portainer (infrastructure)

**Nginx configurations:**

- `/etc/nginx/sites-available/gezielcarvalho.info`
- `/etc/nginx/sites-available/moodle001.gezielcarvalho.info`
- `/etc/nginx/sites-available/moodle002.gezielcarvalho.info`
- `/etc/nginx/sites-available/wp001.gezielcarvalho.info`
- `/etc/nginx/sites-available/qa-infrastructure`

---

## Verification Checklist

After applying the fix:

- [x] Nginx service is running: `sudo systemctl status nginx`
- [x] Apache2 is stopped: `sudo systemctl status apache2`
- [x] Apache2 won't auto-start: `sudo systemctl is-enabled apache2` → disabled
- [x] Port 80 is listening (Nginx): `sudo netstat -tulpn | grep :80`
- [x] Port 443 is listening (Nginx): `sudo netstat -tulpn | grep :443`
- [x] Websites are accessible: `curl -I https://gezielcarvalho.info`
- [x] No binding errors in logs: `sudo journalctl -u nginx -n 50`

---

## Quick Reference Commands

```bash
# Check service status
sudo systemctl status nginx
sudo systemctl status apache2

# Check what's using a port
sudo netstat -tulpn | grep :PORT
sudo lsof -i :PORT

# Stop/Start/Restart services
sudo systemctl stop SERVICE
sudo systemctl start SERVICE
sudo systemctl restart SERVICE

# Enable/Disable auto-start
sudo systemctl enable SERVICE
sudo systemctl disable SERVICE

# Test Nginx configuration
sudo nginx -t

# Reload Nginx (without downtime)
sudo systemctl reload nginx

# View logs
sudo journalctl -u nginx -f
sudo tail -f /var/log/nginx/error.log
```

---

## Lessons Learned

1. **Only run one web server per port** - Either Nginx OR Apache2 on port 80/443
2. **Check service states after system updates** - Package updates may re-enable services
3. **Always verify configs are valid** before assuming port conflicts
4. **Use `netstat` or `ss` to identify port conflicts** quickly
5. **Disable unused services** to prevent resource conflicts

---

**Status:** ✅ **RESOLVED**  
**Solution:** Disabled Apache2, started Nginx  
**Impact:** All websites now accessible via Nginx
