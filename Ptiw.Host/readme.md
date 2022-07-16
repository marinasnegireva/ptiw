# Chains

## Clinic appointments notification chain


```mermaid
graph LR
A[GetAppointments] --> B[ReactionManager]
B -- for each user--> C(FindAppointmentsForUser)
C --> D(ReactionManager)
D --> E(Notifier)