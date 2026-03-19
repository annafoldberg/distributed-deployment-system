# Distributed Deployment System
A distributed system for automated deployment of software releases to customers.

---
## Deployment Manager API
API responsible for managing deployment state and coordinating release deployments. Exposes endpoints for reading and updating each customer's current and desired software version, and retrieves new releases from GitHub when requested.

---
## Deployment Manager CLI
CLI used internally to manage deployments. Shows the current and desired software version for each customer and allows update of desired version.

---
## Deployment Manager Agent
Background worker periodically checking for changes in customer's desired software version, comparing it with the currently installed version, and when mismatch is detected, downloads and installs new release.

---
## Related repository
Demo updatable console application can be found at  
[demo-updatable-app](https://github.com/annafoldberg/demo-updatable-app).