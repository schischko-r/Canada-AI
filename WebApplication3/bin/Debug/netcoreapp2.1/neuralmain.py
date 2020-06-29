import requests
import json
import math as m
import re

from dictmain import dictmain

def variable(prov, hectares, town, forest, water):
    d = dictmain()
    variable = (d.variable[prov]['Town']*7.75*hectares*town/100 + (d.variable[prov]['Road']*2.96 + d.variable[prov]['Railroad']*3.05)*hectares*forest/100 - water * hectares*1.25/100)*8.65
    if variable <=10:
      return 0
    return variable

def fixedC(prov, hectares, extra, water):
    scoring_uri = 'http://7c8d350d-eb8a-4a0c-8839-aacc0e79d5e3.westeurope.azurecontainer.io/score'
    key = ''

    d = dictmain()

    data = {"data":
          [
              [
                prov,                                             # КОДИФИКАЦИЯ ПРОВИНЦИИ
                d.HARDWOOD[prov],                                 # HARDWOOD
                d.SOFTWOOD[prov],                                 # SOFTWOOD
                1 - d.HARDWOOD[prov] - d.SOFTWOOD[prov],          # UNDEFIGNED
                hectares,                                         # HECTARES BURNED
                extra                                             # ПОСТОРОННИЕ ЗАТРАТЫ
              ],
              
          ]
          }

    input_data = json.dumps(data)

    headers = {'Content-Type': 'application/json'}
    headers['Authorization'] = f'Bearer {key}'

    resp = requests.post(scoring_uri, input_data, headers=headers)
    fixed = float(re.findall(r"\d+\.\d+", resp.text)[0])

    if water != 0:
      fixed = (fixed * hectares / d.HECT_COEF[prov])*(-m.log(water/100))/2
    else:
      fixed = (fixed * hectares / d.HECT_COEF[prov])
    if fixed <=10:
      return 0
    return fixed

def main(prov, hectares, town, water):
  forest = 100 - town - water

  var = variable(prov,hectares,town,forest,water)
  fixed = fixedC(prov, hectares, var, water)
  return fixed, var
