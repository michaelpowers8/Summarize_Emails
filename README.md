# Summarize_Emails Using ChatGPT

This program is designed to use locally downloadable GPT models from https://ollama.com/search to read and summarize
the contents of emails. Those summaries are currently added to individual text files. Every single email gets its own
text file with the following information:

- A prompt for the AI model full of instructions to follow
- The original text content of the email excluding email attachment content and who the email was sent to/from
- The time it took for the AI to generate a response
- The full response of the AI.

This program iterates through all emails that were previously classified as Actionable. Every actionable email is 
prioritized based on how many actionable phrases were found in the contents of the email.