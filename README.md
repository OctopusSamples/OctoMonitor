# OctoMonitor

This repository contains sample serverless applications for processing webhook notifications from [Octopus Subscription Events](https://octopus.com/docs/administration/managing-infrastructure/subscriptions).

# Support

This repository is intended for sample purposes only and is provided as-is. If there are any issues please do not contact Octopus support.

# Architecture

Each sample will follow the same basic architecture.

1. An API application to accept a message via a webhook from Octopus Deploy and save it to a queue.
2. A Processing application to process messages from the queue.

Each application is used to monitor _one_ Octopus Deploy instance.  

# Infrastructure

Included in this sample is the Terraform script to build up all the necessary infrastructure required to support these applications in a cloud provider. Currently only AWS is supported.