# Improved Twitter Sentiment Analysis Using Naive Bayes and Custom Language Model: Microsoft Azure as Case Study

Automates twitter promotion sentiment analysis of Microsoft Azure based on defined attributes. Calls Microsoft Cognitive Service LUIS API to get predicted intent. 


Program functionalities
- Produces random samples of tweets from original raw data and keeps track of tweets that have been fetched
- Data cleansing: 
- Data Attribute Analysis based on final WEKA model attributes (in section VII of paper)
       1. Regex to read and understand the data better for keywords analysis too 
       2. All keywords stored in libraries to easily identify if such keywords exist 
       3. URL Analysis: To identify whether a URL was “Microsoft-specific,” we looked for the words “Microsoft” or “Azure” in the URL and we fetched the title of the URL article link to see if the words “Microsoft” or “Azure” appeared there. We also ensured there was no mention of Azure’s competitor (AWS, Amazon etc.). 
       4. Programmed automatic call to custom language model developed with LUIS (Language Understanding Intelligent Services) API so tweets could automatically receive intent prediction and score 
- Writes all data attributes results into a CSV file to be uploaded into WEKA (CSV Writer)
