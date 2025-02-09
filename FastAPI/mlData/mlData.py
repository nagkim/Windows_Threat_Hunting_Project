
from fastapi import FastAPI
from pydantic import BaseModel
import joblib
import numpy as np

# replace with your path
model = joblib.load('C:\\Users\\nagkim\\source\\repos\\mlData\\mlData\\random_forest_model_11_features.joblib') 


app = FastAPI()


class MalwarePredictionInput(BaseModel):
    features: list


@app.post("/predict")
def predict(data: MalwarePredictionInput):
    
    input_features = np.array(data.features).reshape(1, -1)
   
    prediction = model.predict(input_features)
    
    return {"prediction": int(prediction[0])}
