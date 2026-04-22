# Architecture Diagrams

## Overall Azure Architecture

```mermaid
graph TB
    User[User/Browser] --> AFD[Azure Front Door - Optional]
    AFD --> AppService[Azure App Service]
    
    subgraph "App Service Plan B1"
        AppService --> Production[Production Slot]
        AppService --> Staging[Staging Slot]
    end
    
    Production --> MI[Managed Identity]
    Staging --> MI
    
    MI --> KV[Azure Key Vault]
    MI --> SQL[Azure SQL Database]
    
    Production --> AI[Application Insights]
    Staging --> AI
    
    AI --> LA[Log Analytics Workspace]
    
    KV --> Secrets[(Secrets<br/>- DB Connection<br/>- API Keys)]
    SQL --> DB[(ProductDb)]
    
    GitHub[GitHub Repository] -.->|Push| Actions[GitHub Actions]
    Actions -.->|Deploy| Production
    Actions -.->|Deploy| Staging
    
    style Production fill:#4CAF50
    style Staging fill:#FF9800
    style KV fill:#0078D4
    style SQL fill:#CC2927
    style AI fill:#68217A
```

## CI/CD Pipeline Flow

```mermaid
flowchart LR
    A[Developer Push] --> B[GitHub Actions Triggered]
    B --> C[Checkout Code]
    C --> D[Restore Dependencies]
    D --> E[Build Solution]
    E --> F[Run Tests]
    F --> G{Tests Pass?}
    G -->|No| H[Fail Build]
    G -->|Yes| I[Publish Artifacts]
    I --> J[Upload to GitHub]
    J --> K[Download Artifacts]
    K --> L[Deploy to Azure]
    L --> M[Health Check]
    M --> N{Healthy?}
    N -->|No| O[Rollback]
    N -->|Yes| P[Deployment Complete]
    
    style G fill:#FF9800
    style N fill:#FF9800
    style P fill:#4CAF50
    style H fill:#F44336
    style O fill:#F44336
```

## Blue-Green Deployment

```mermaid
graph LR
    subgraph "Before Swap"
        LB1[Load Balancer] --> Prod1[Production Slot<br/>Version 1.0]
        LB1 -.->|No Traffic| Stage1[Staging Slot<br/>Version 2.0]
    end
    
    subgraph "After Swap"
        LB2[Load Balancer] --> Prod2[Production Slot<br/>Version 2.0]
        LB2 -.->|No Traffic| Stage2[Staging Slot<br/>Version 1.0]
    end
    
    style Prod1 fill:#4CAF50
    style Stage1 fill:#FF9800
    style Prod2 fill:#4CAF50
    style Stage2 fill:#FF9800
```

## Authentication Flow (Managed Identity)

```mermaid
sequenceDiagram
    participant App as App Service
    participant MI as Managed Identity
    participant AAD as Azure AD
    participant KV as Key Vault
    participant SQL as SQL Database
    
    App->>MI: Request Token
    MI->>AAD: Authenticate with MI
    AAD->>MI: Return Access Token
    MI->>App: Provide Token
    
    App->>KV: Get Secret (with token)
    KV->>AAD: Validate Token
    AAD->>KV: Token Valid
    KV->>App: Return Secret Value
    
    App->>SQL: Connect (with connection string)
    SQL->>AAD: Validate MI (if using AAD auth)
    AAD->>SQL: Identity Valid
    SQL->>App: Connection Established
```

## Resource Dependencies

```mermaid
graph TD
    RG[Resource Group] --> ASP[App Service Plan]
    RG --> KV[Key Vault]
    RG --> SQL[SQL Server]
    RG --> AI[Application Insights]
    RG --> LA[Log Analytics]
    
    ASP --> AS[App Service]
    AS --> Prod[Production Slot]
    AS --> Stage[Staging Slot]
    
    SQL --> DB[SQL Database]
    SQL --> FW[Firewall Rules]
    
    AI --> LA
    
    Prod --> MI1[Managed Identity]
    Stage --> MI2[Managed Identity]
    
    MI1 --> KVAccess[Key Vault Access]
    MI2 --> KVAccess
    
    KVAccess --> KV
    
    style RG fill:#0078D4
    style AS fill:#4CAF50
    style KV fill:#FF9800
    style SQL fill:#CC2927
```

## Monitoring & Logging

```mermaid
graph TB
    App[App Service] --> AI[Application Insights]
    
    subgraph "Application Insights"
        AI --> Requests[Requests]
        AI --> Exceptions[Exceptions]
        AI --> Dependencies[Dependencies]
        AI --> Custom[Custom Events]
    end
    
    AI --> LA[Log Analytics]
    
    subgraph "Log Analytics Workspace"
        LA --> Queries[KQL Queries]
        LA --> Dashboards[Dashboards]
        LA --> Alerts[Alert Rules]
    end
    
    Alerts --> AG[Action Groups]
    
    subgraph "Notifications"
        AG --> Email[Email]
        AG --> SMS[SMS]
        AG --> Webhook[Webhook]
    end
    
    Dashboards --> Portal[Azure Portal]
    
    style AI fill:#68217A
    style LA fill:#0078D4
    style Alerts fill:#F44336
```

## Scaling Strategy

```mermaid
graph LR
    M[Metrics] --> CPU{CPU > 70%}
    M --> Memory{Memory > 80%}
    M --> Requests{Requests > 1000/min}
    
    CPU -->|Yes| ScaleOut[Scale Out<br/>+1 Instance]
    Memory -->|Yes| ScaleOut
    Requests -->|Yes| ScaleOut
    
    CPU -->|No| Check1[Check Scale Down]
    Memory -->|No| Check2[Check Scale Down]
    Requests -->|No| Check3[Check Scale Down]
    
    Check1 --> Low{CPU < 30%<br/>for 10 min}
    Check2 --> Low
    Check3 --> Low
    
    Low -->|Yes| ScaleIn[Scale In<br/>-1 Instance]
    Low -->|No| Maintain[Maintain Current]
    
    ScaleOut --> Min{Min = 1<br/>Max = 3}
    ScaleIn --> Min
    
    style ScaleOut fill:#4CAF50
    style ScaleIn fill:#FF9800
    style Maintain fill:#2196F3
```

## Network Architecture (with Optional Front Door)

```mermaid
graph TB
    Internet[Internet] --> DNS[DNS]
    DNS --> FD[Azure Front Door<br/>Optional]
    
    FD --> WAF[Web Application Firewall]
    WAF --> Cache[Edge Caching]
    
    Cache --> Region1[East US<br/>App Service]
    Cache --> Region2[West US<br/>App Service<br/>Optional]
    
    Region1 --> VNET1[Virtual Network<br/>Optional]
    Region2 --> VNET2[Virtual Network<br/>Optional]
    
    VNET1 --> SQL1[(Primary SQL)]
    VNET2 --> SQL2[(Secondary SQL<br/>Read Replica)]
    
    SQL1 -.->|Geo-Replication| SQL2
    
    style FD fill:#0078D4
    style WAF fill:#F44336
    style Region1 fill:#4CAF50
    style Region2 fill:#FF9800
```

---

## Cost Breakdown (Monthly Estimates)

| Resource | SKU | Estimated Cost |
|----------|-----|----------------|
| App Service Plan | B1 | $13.14/month |
| Azure SQL Database | Basic (2GB) | $4.90/month |
| Key Vault | Standard | $0.03/10k ops |
| Application Insights | First 5GB | Free |
| Log Analytics | First 5GB | Free |
| **Total** | | **~$18-20/month** |

### Scaling Costs

| Tier | Monthly Cost | Use Case |
|------|--------------|----------|
| F1 (Free) | $0 | Dev/Test only |
| B1 (Basic) | $13 | Small apps, staging |
| S1 (Standard) | $70 | Production, autoscale |
| P1V2 (Premium) | $146 | High performance |

---

## Security Layers

```mermaid
graph TB
    subgraph "Network Security"
        N1[HTTPS Only]
        N2[TLS 1.2+]
        N3[IP Restrictions]
    end
    
    subgraph "Identity & Access"
        I1[Managed Identity]
        I2[RBAC]
        I3[Key Vault]
    end
    
    subgraph "Data Protection"
        D1[SQL Encryption at Rest]
        D2[Encryption in Transit]
        D3[Secret Rotation]
    end
    
    subgraph "Monitoring"
        M1[Application Insights]
        M2[Audit Logs]
        M3[Security Alerts]
    end
    
    style N1 fill:#4CAF50
    style I1 fill:#4CAF50
    style D1 fill:#4CAF50
    style M1 fill:#4CAF50
```
