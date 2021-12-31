# Simoona

[![Build status](https://ci.appveyor.com/api/projects/status/j5y450yftsvso2je?svg=true)](https://ci.appveyor.com/project/Simoona/simoona-ks9ka) [![Join the chat at https://gitter.im/simoona-oss/community](https://badges.gitter.im/simoona-oss/community.svg)](https://gitter.im/simoona-oss/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Simoona is an open source intranet solution powered by Visma. We created Simoona so your company could make its communication more intuitive, more efficient, more empowering, and more fun. In the open source version, you will be able to use the core functions of Simoona. Want to add a feature? No problem, you can either get the full version from us or edit the open source version and make it the best intranet solution for your team. Focus on what's important and create an atmosphere of respect and acknowledgment.

![Simoona demo](https://github.com/VismaLietuva/simoona-docs/raw/master/img/simoona-demo.gif "Simoona demo")

## Simoona Features

<details>
  <summary>Wall feature</summary>

  Only the option of speaking up and sharing what one cares about creates the atmosphere of freedom, equality and knowing that every employee matters and will be heard.
  
  Simoona Wall helps to understand what employees are thinking and caring about. Give your people the ability to share their thoughts and participate in lively debates and discussions. Having a voice matters to engagement.
  
  Simoona Wall works as simple as any social media wall. Employees can write posts of any length, add pictures, video links or gif images. Fellow employees can react to the conversation by liking it or adding a comment. The Comment section allows users to add pictures, videos and links.

  ![Wall feature](https://github.com/VismaLietuva/simoona-docs/raw/master/img/feature-walls.gif "Wall feature")
</details>

<details>
  <summary>Employees feature</summary>

  It sometimes gets hard to wrap your head around all the new faces in the office. That's why Simoona has an Employee list, which will help you learn more about your colleagues.
  
  The list consists of the employee’s name and surname, birth date, position at the company, telephone number and work hours. If you want to start down the path of learning more about your colleagues, you can visit their pages. There you will be able to find a photo, their email, when they started working at the company, their senior, their workplace, what projects they are working on, and something about themselves if they are willing to share.
  
  So, all of this will make it so much easier to keep track of who is who and more. This feature is especially valuable for bigger companies or those scattered across different cities or countries.
  
  ![Employees feature](https://github.com/VismaLietuva/simoona-docs/raw/master/img/feature-employees.gif "Employees feature")
</details>

<details>
  <summary>Kudos feature</summary>

  Social contribution needs to be quantified – that’s why Simoona has a built-in Kudos reward system. Every employee can assign certain points per good deed and demonstrate the recognition to their colleagues.
  
  It’s a cornerstone of social status (and thus peer pressure): doing the right thing for a team not only is good, but it is also tangible and clearly visible to everyone. Any organization can decide on its very own Kudos system design. The Kudos employee reward system provides a gamification dimension to organizational culture. The important thing is to create a work culture that provides recognition for social contributors and achievers.
  
  Kudos feature is fast and easy to use. A user can pick any fellow employee and set the amount of donated Kudos, justifying it by describing the good deed or work done by the Kudos recipient.

  ![Kudos feature](https://github.com/VismaLietuva/simoona-docs/raw/master/img/feature-kudos.gif "Kudos feature")
</details>

## Project Structure

Simoona consists of two main parts:

* Front-end webapp - purely AngularJS application (no server-side code),
* Back-end app - ASP.NET MVC+WebApi project, with EntityFramework code-first database.

## Install from Binaries

The easiest way to install and try out Simoona is from pre-compiled binaries.
To do it, please follow the [Install from binaries guidelines](LocalSetup.md).

## Install from Source Code

If you are planning to contribute or change Simoona code, we suggest following the [Installation from source code guidelines](build/README.md). Just head over there and follow the instructions.

## Developing and Contributing

If you are planning to contribute to the Simoona project, please look into more detailed documentation about Simoona parts:

1. [Front-end webapp](src/webapp/README.md)
    * Gulp Command List
    * Configuration
    * SSL Cerificate
    * Web Config

1. [Back-end app](src/api/README.md)
    * QuickStart
    * SMTP Client Setup
    * Configuration
    * Optional Features
