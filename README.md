# Inbox Outbox Pattern
The aim of this project is to process the Outbox-Inbox pattern by following the best practices on .NET Core 8

![outbox-inbox.drawio.png](https://github.com/tahapek5454/Inbox-Outbox-Pattern/blob/main/outbox-inbox.drawio.png)


## Scenario
+ Before one service sends a message to another service, it first records that message in the database.
    + This ensures persistence for the message from a security perspective. Even if the message is lost during the process, the other service's actions will be recorded in this table.
+ Subsequently, an application is deployed to process each row in this table and send messages to the target service.
+ Thus, the messages will be sent to the target service through this application, ensuring guaranteed processing.

**NOTE:** This design, ensuring the reliability of the message from the sender's perspective, is called the **Outbox Design Pattern**. The terminological name for the table physically holding the messages to be sent in this pattern is **Outbox Table**, and the name of the application responsible for sending these messages to the target service is **Outbox Publisher Application**.

+ Later, before the message is processed by the receiving service, it is recorded in a table called **Inbox Table**. Similar to the Outbox Table, this ensures the integrity of both the messages and whether they have been processed.

**NOTE:** This design, ensuring the processing reliability of the message from the receiver's perspective, is called the **Inbox Design Pattern**, and in this pattern, there is an **Inbox Table** physically holding the messages to be processed.

+ In essence, in microservices architecture, the reliability of messages used in communication between services is crucial. Therefore, measures are taken with **Outbox Design Pattern** and **Inbox Design Pattern** to ensure the reliability of these messages.

## Outbox Design Pattern
In systems that communicate asynchronously, the Outbox Design Pattern is a crucial design for handling communication processes between services (or between a service and a message broker). It effectively prevents potential data losses and ensures data consistency between services in case of issues such as the connection being lost before the message is delivered, errors occurring, or unexpected physical or software interruptions during the process.

+ For example, suppose you receive an order and need to send it for stock checking. However, an error occurs, and now you need to apply Compensable Transaction to maintain consistency. But what happened is you lost the order. To prevent this loss, you will use the Outbox Design Pattern to avoid losing the order, and when things are back on track, the relevant service consumes the order.

+ Ultimately, the Outbox Design Pattern ensures the integrity by storing messages in the Outbox table during asynchronous processes, preventing potential communication and data transfer problems caused by communication interruptions, system failures, or connection disruptions.

**Note:** In microservices approaches, each service with critical messages should have its own Outbox table. Messages should be placed in this table first and then sent to the target service or message broker through a publisher.

## When Should Outbox Design Pattern Be Used?
+ Outbox pattern is particularly useful when two operations are performed simultaneously in a service.
+ Consider an Order Service where both a record is added to the Orders table for a received order and an event named OrderCreatedEvent indicating that the order has been created is written to the message broker. If a service performs two different operations like this during its processes, we call this situation **Dual Write**.
+ In situations where Dual Write occurs, problems can arise if the record is not written to the database or not sent to the message broker. This situation creates significant data inconsistencies, leading to inconsistencies.
+ To minimize the possible inconsistency, the Outbox pattern can play a vital role. The Outbox pattern is based on storing the operations to be performed after a change in the database as a task within the database. Thus, we perform the operation we want to do in the database within the transaction and leave a mark for the action we will perform after this operation.

**Note:** If there is Dual Write, the Outbox pattern should be implemented.

## How Should Messages Be Published in the Outbox Design Pattern?
+ Pulling Publisher 
    + It is a method of developing an application that queries and publishes messages in the Outbox table at certain time intervals.
+ Transaction Log Tailing
    + It is a method of reading the transaction logs of the Outbox table's database to capture changes and publish messages.

## Idempotent Issue
+ Suppose we recorded the relevant event in the Outbox table, and the publisher sent it, but there was an issue in the database, and we couldn't mark or delete the event. When the publisher is triggered again, it will send the same event again, which is the idempotent issue.
+ The solution here is applied on the consumer side. Consumers should be designed as **idempotent** to eliminate this possible error.

**What is Idempotent:** It ensures that a message, even if published multiple times, has the same functionality from the consumer's perspective.

+ To ensure this, a special key, token, or ID is determined in the message to be published. Later, the consumer can understand whether the message has been consumed before.
