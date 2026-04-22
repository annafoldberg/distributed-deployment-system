apiVersion: v1
kind: Secret
metadata:
  name: api-secret
  namespace: deployment-manager
stringData:
  Database__Password: "<db-password>"
  AgentSeeding__Agents__0__ApiKey: "<agent-api-key>"
  AgentSeeding__Agents__1__ApiKey: "<agent-api-key>"
  GitHub__Token: "<github-token>"
  
# Source: https://kubernetes.io/docs/concepts/configuration/secret/