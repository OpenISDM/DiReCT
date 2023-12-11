# Alerting and Detection Framework

## ADS Framework
Prior to the development and adoption of the ADS framework, we faced major challenges with development of alerting strategies. There was a lack of rigor around the creation, development, and implementation of an 
alert, which led to sub-optimal alerts going to production without documentation or peer-review. Over time, some of the alerts gained a reputation of being low-quality, which led to fatigue, alerting apathy,
or additional engineering time and resources.

To combat the issues and deficiencies previously noted, we developed an ADS framework which is used for all alerting development. This is a natural language template which helps frame hypothesis generation, testing
and management of new ADS.

The ADS Framework has the following sections, each which must be completed prior to production implementation:

* Goal
* Categorization
* Strategy Abstract
* Technical Context
* Blind Spots and Assumptions
* False Positives
* Validation
* Priority
* Response

Each section is required to successfully deploy a new ADS, and guarantees that any given alert will have sufficient documentation, will be validated for durability, and reviewed prior to production deployment.

Each production or alert is based on the ADS framework is stored in a durable, version-controlled, and centralized location.
