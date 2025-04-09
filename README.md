# Summarize_Emails Using ChatGPT

This program leverages locally downloadable GPT models from [Ollama](https://ollama.com/search) to read and summarize email content. Summaries are saved to individual text files, with each file containing:

- A prompt given to the AI model
- The original email text (excluding attachments and sender/recipient data)
- The generation time for the AI response
- The full AI-generated summary

---

## Processing Logic

The system iterates through all emails previously marked as *Actionable*. These are prioritized based on the number of actionable phrases detected within their content.

The current setup handles ~30,000 actionable emails. With an average processing time of 45 seconds per email, full completion takes approximately 16 days of continuous runtime.

---

## Environment & Model

- **Model:** `llama3.1:latest`
- **OS:** Windows 11 Pro  
- **Hardware:**  
  - 32 GB RAM  
  - Intel(R) Xeon(R) Gold 6430 @ 2.10 GHz  
  - 10 Cores, 10 Logical Processors  
  - No GPU

---

## Project Purpose

The primary objective is to optimize prioritization and response to high volumes of inbound email. By summarizing and surfacing high-impact items, the system enhances operational efficiency.

---

## Disclaimer

This repository is a portfolio backup. All source code and outputs have been modified to remove or obscure client-sensitive data.

To view a demonstration of the full project, contact the author at:  
📧 **micpowers98@gmail.com**
