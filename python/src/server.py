import json
import os

import flask
from flask import Flask, render_template
from flask_cors import CORS

from .naive_api_client import NaiveApiClient

app = Flask(__name__)
CORS(app)

secret = os.environ.get('API_SECRET')
client_id = os.environ.get('API_CLIENT_ID')
product_type = os.environ.get('API_PRODUCT_TYPE', 'employment')

api_client = NaiveApiClient(
    api_url=os.environ.get('API_URL', 'https://prod.citadelid.com/v1/'),
    secret=secret,
    client_id=client_id,
)

if not secret or not client_id:
    raise Exception("Environment MUST contains 'API_SECRET' and 'API_CLIENT_ID'")

print("=" * 40, "ENVIRONMENT", "=" * 40, "\n",
      api_client.API_URL, "\n",
      json.dumps(api_client.API_HEADERS, indent=4), "\n",
      "=" * 94, "\n", )


@app.context_processor
def inject_product_type():
    return dict(product_type=product_type, )


@app.route('/')
def index():
    """Just render example with bridge.js"""
    if product_type == 'income':
        return render_template('income.html')
    else:
        return render_template('employment.html')

@app.route('/getBridgeToken', methods=['GET'])
def create_bridge_token():
    return api_client.get_bridge_token()

@app.route('/createAccessToken', methods=['POST'])
def create_access_token():
    """Handler to exchange public_key from widget check with access_token"""
    json_data = flask.request.json

    return {
        'access_token': api_client.get_access_token(
            public_token=json_data.get('public_token'))
    }


@app.route('/getVerifications/<public_token>', methods=['GET'])
def get_verification_info_by_token(public_token: str):
    """ getVerificationInfoByToken """

    # First exchange public_token to access_token
    access_token = api_client.get_access_token(public_token)

    # Use access_token to retrieve the data
    if product_type == 'employment':
        verifications = api_client.get_employment_info_by_token(access_token)
    elif product_type == 'income':
        verifications = api_client.get_income_info_by_token(access_token)
    else:
        raise Exception('Unsupported product type!')
    return verifications


if __name__ == '__main__':
    app.debug = True
    app.run()
