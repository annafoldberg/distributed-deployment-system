apiVersion: v1
kind: Secret
metadata:
  name: mssql-secret
  namespace: deployment-manager
stringData:
  saPassword: "<sa-password>"
  
# Source: https://kubernetes.io/docs/concepts/configuration/secret/