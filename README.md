# hack-the-planet-2015
## Turn it Down
- Download the app for iOS. Enter up to 3 roommates' phone # and email into the app (these will be saved and available for every session). Before bed, press the "Go to Sleep" button in the app. Your device will be listening for loud noise deviations that go on for a consistent period of time.

- We use the Twilio API to send messages to your roommates politely (at first) asking them to keep it down. We will send a total of 4 text messages throughout the night that increase in desperation if the sound decibels increase.

- If your roommate still can't turn it down, our app's final communication with them is to send them an email through the SendGrid API explaining the importance of sleep, complete with an infographic.

- In the morning, press the 'Wake Up' button to finish the session.
