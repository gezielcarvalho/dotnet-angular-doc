# Introduction to Sabre Shop (SShop)

Sabershop is a simple e-commerce application built with DotNet and Angular. It is a simple application that allows users to view and edit a list of products and categories.
We will organize our sample project - the SShop - several iterations. Each iteration will be a new branch in the repository. The first iteration will be the current state of the project, inheriting all the features developed so far. The second iteration will be the first step in the development of the project. The third iteration will be the second step in the development of the project. And so on.

## Iteration 1 - The Current State of the Project

1. Actual state of the project
2. Initial documentation
   1. Major components (functional areas) that are needed.
      1. Supplier Information Subsystem
      2. Product Information Subsystem
   2. Iterations.
      1. Identify the tasks required for the iteration.
      2. Organize and sequence these tasks into a schedule.
      3. Identify required resources and assign people to tasks

### Work Breakdown Structure (WBS):

1. Discover and understand the details of all aspects of the problem.
   1. Identify and define use cases
   2. Identifying Domain Classes
   3. Identify and define information requirements
   4. Develop workflows and descriptions for the use cases
2. Design the components of the solution to the problem
   1. Design input screens, output screens, and reports
   2. Design and build the database (attributes, keys and indexes)
   3. Design the overall architecture of the solution
   4. Design program details
3. Build the components and integrate everything into the solution
   1. Code and unit test GUI layer programs
   2. Code and unit test Logic layer programs
4. Perform all system level tests and then deploy the solution
   1. Perform system functionality testing
   2. Perform system integration testing
   3. Perform User Acceptance Testing
   4. Deploy the solution

### Identifying Use Cases

Document what the users need to do with the system, a case or situation where the system is used.

| Use Case                         | Description                                                     |
| -------------------------------- | --------------------------------------------------------------- |
| Look up supplier                 | Using supplier name, find supplier information and contacts     |
| Enter/update supplierinformation | Enter (new) or update (existing) supplier information           |
| Look up contact                  | Using contact name, find contact information                    |
| Enter/update contact information | Enter (new) or update (existing) contact information            |
| Look up product information      | Using description or supplier name, look up product information |
| Enter/update product information | Enter (new) or update (existing) product information            |
| Upload product image             | Upload images of the merchandise product                        |

### Identifying Domain Classes

Domain classes are the categories of objects identified, much like a table in a database represents the category of the records it contains

| Domain Class | Attributes                                            |
| ------------ | ----------------------------------------------------- |
| Supplier     | supplierName, address, city, state, zip, phone, email |
| Contact      | contactName, phone, email                             |
| Product      | description, price, image                             |
| Category     | categoryName, description                             |
| Image        | imageName, imageDescription, imageFile, imageCaption  |

### Identifying Information Requirements

Detail the use cases and domain classes that are scheduled to be implemented in the first iteration.

## Iteration 2 - The First Step in the Development of the Project

1. To be defined.
