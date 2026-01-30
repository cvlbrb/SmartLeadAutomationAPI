import requests
import pandas as pd
import streamlit as st

st.title("💹 Rastreador de Criptomoedas")

moeda = st.selectbox("Selecione a Criptomoeda:", ["bitcoin", "ethereum", "dogecoin"])

url = f"https://api.coingecko.com/api/v3/coins/{moeda}/market_chart"
params = {"vs_currency": "usd", "days": 7}

response = requests.get(url, params=params)

st.write("Status code:", response.status_code)

if response.status_code == 200:
    dados = response.json()
    df = pd.DataFrame(dados["prices"], columns=["timestamp", "price"])
    df["timestamp"] = pd.to_datetime(df["timestamp"], unit="ms")
    df.set_index("timestamp", inplace=True)

    st.line_chart(df)
    st.write(df.tail())
else:
    st.error("Erro ao buscar dados da API")

