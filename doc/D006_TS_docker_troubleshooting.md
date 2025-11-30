# Docker Troubleshooting Guide

## Network Issues

### Error: "all predefined address pools have been fully subnetted"

**Problem:**

When running `docker-compose up`, you may encounter the following error:

```
[+] Running 3/3
 ✔ backend                                     Built          0.0s
 ✔ frontend                                    Built          0.0s
 ✘ Network dotnet-angular-doc_edm-network  Error          0.0s
failed to create network dotnet-angular-doc_edm-network: Error response from daemon: all predefined address pools have been fully subnetted
```

**Cause:**

Docker has run out of available subnet addresses for creating networks. This typically happens when there are many unused networks consuming the address pool.

**Solution:**

Remove all unused Docker networks to free up subnet addresses:

```bash
# Remove all unused networks
docker network prune -f

# Alternatively, view networks first before pruning
docker network ls
docker network prune
```

After pruning unused networks, run `docker-compose up` again.

**Prevention:**

Regularly clean up Docker resources when you're done with projects:

```bash
# Remove unused networks
docker network prune

# Remove unused containers
docker container prune

# Remove unused images
docker image prune

# Remove all unused resources (containers, networks, images, volumes)
docker system prune -a
```
