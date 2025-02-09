# Windows_Threat_Hunting_Project
Windows threat hunting using machine learning

# Virus Detection System

## Overview
This project is a **Virus Detection System** designed to identify and analyze malicious software (malware) on Windows operating systems. It employs various methods such as monitoring scheduled tasks, startup folders, system services, and network traffic. The system integrates **machine learning models** and **third-party APIs** for enhanced detection accuracy, providing a **user-friendly interface** for real-time monitoring and manual file scanning.

## Features

- **Startup Monitoring**: Scans registry keys and startup folders for suspicious programs.
- **Task Scheduler Analysis**: Detects anomalies in scheduled tasks using IBM X-Force Exchange API.
- **Service Inspection**: Identifies harmful services running in the background.
- **Manual File Scanning**: Allows users to upload files for malware analysis via Hybrid Analysis API.
- **Smart Scan**: Combines database lookups and machine learning (Random Forest) to classify processes.
- **AI Integration**: Leverages OpenAI GPT-3.5 Turbo for contextual threat analysis.


## Technologies Used

- **Programming Language**: C# (.NET Framework)
- **APIs**: Hybrid Analysis, IBM X-Force Exchange, FastAPI, OpenAI GPT-3.5 Turbo
- **Machine Learning**: Random Forest algorithm (scikit-learn)
- **Database**: SQL for storing malware signatures and process data


## Installation

### Clone the Repository
```bash
git clone https://github.com/nagkim/Windows_Threat_Hunting_Project.git
cd Windows_Threat_Hunting_Project
```

### Install Dependencies

Ensure **.NET Framework 4.8** is installed.

Install required NuGet packages...


### API Keys
Register for API keys from **Hybrid Analysis**, **IBM X-Force Exchange**, and **OpenAI**.


### Build and Run
1. Open the solution in **Visual Studio**.
2. Build the project.
3. Run `Interface.sln`.

## Usage

### Quick Scan
- Automatically scans startup programs and highlights suspicious entries (**red = malicious, orange = risky, green = benign**).

### Task Manager
- Lists scheduled tasks and flags anomalies.
- Use the **Delete Task** button to remove suspicious entries.

### File Scan
- Upload files manually to check for malware.
- Results show threat level (e.g., **"Malicious" or "No Threat"**).

### Services
- Monitor running services and their resource usage.
- Suspicious services are flagged based on file paths.

### Smart Scan
- Real-time process monitoring with **color-coded results** (**green = benign, yellow = risky, red = malware**).

### Ask GPT
- Query **OpenAI GPT** for contextual insights on flagged processes or files.

## Machine Learning Model

- **Algorithm**: Random Forest (trained on **PE header features** and **process behavior data**).
- **Dataset**: **19,611 samples** (14,599 malicious, 5,012 benign) from VirusShare and MalwareBazaar.
- **Performance**:
  - **Accuracy**: 98%
  - **Precision**: 98% (Malicious), 98% (Benign)
  - **F1-Score**: 98% (Malicious), 95% (Benign)
  - **False Positives**: 42 benign processes misclassified (1.1% error rate)

## Test Results

- **File Scan**: Correctly identified **100%** of test malware samples.
- **Task Detection**: Successfully flagged **2914/2920** malicious tasks.
- **False Positives**: **42 benign processes** misclassified (1.1% error rate).


## Acknowledgments

- **Malware datasets**: MalwareBazaar, VirusShare
- **APIs**: Hybrid Analysis, IBM X-Force Exchange, OpenAI

**Note:** This project is for educational purposes only.
