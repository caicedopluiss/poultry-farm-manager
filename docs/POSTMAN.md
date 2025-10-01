# API Testing with Postman

## Overview

This document explains how to test the Poultry Farm Manager API using Postman collections and environment files located in the `/postman` directory.

## Prerequisites

-   [Postman](https://www.postman.com/downloads/) installed
-   API server running locally or deployed

## Setup Instructions

### 1. Import Collection

1. Open Postman
2. Click **Import** button
3. Select the collection file from `/postman/PoultryFarmManager.postman_collection.json`
4. Click **Import**

### 2. Import Environment

1. Click the **Environments** tab
2. Click **Import**
3. Select the environment file from `/postman/PoultryFarmManager.postman_environment.json`
4. Click **Import**

### 3. Select Environment

1. In the top-right corner, select the imported environment from the dropdown
2. Update environment variables as needed (API base URL, tokens, etc.)

## Usage

### Running Individual Requests

1. Navigate to the imported collection
2. Select any request
3. Click **Send**

## Environment Variables

Update these variables in your environment:

-   `host_url`: API server URL
-   `api_version`: API version
