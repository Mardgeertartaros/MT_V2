
from itertools import groupby

import joblib
import polars as pl
import csv
import os
import glob
import pandas as pd
import sys


def load_data(fname):
    # In first 5-7 rows of raw data there is no useful value, after those rows is header,
    # after header may be there are also 1-2 rows null
    with open(fname) as csv_file:
        row = csv.reader(csv_file, delimiter=',')
        # Here we use 'fCurrentScaled' as unique value to find the header
        # Read each row and find if there is 'fCurrentScaled' in this row, if yes, then this row is header
        header_ini = next(row)
        while 'fCurrentScaled' not in header_ini:
            header_ini = next(row)
        # print(header_ini)
    # We don't need all columns, here are all columns what we will select
    u_col = ['fCurrentScaled', 'nSchneidenzahler', 'nSchritt']

    df_nschritt520_550 = pl.scan_csv(fname,
                                     has_header=False,
                                     skip_rows=8,
                                     ignore_errors=True,
                                     with_column_names=lambda col: [col for col in header_ini]) \
        .select(u_col) \
        .filter(pl.col('nSchritt').is_between(520, 550, include_bounds=True)).collect().to_pandas()

    return df_nschritt520_550


class Prediction:
    def __init__(self, filepath):
        self.filepath = filepath

    def prediction(self):
        # find model's path in one folder
        #model_path = sorted(glob.glob("Desktop/*.save"))[0]
        model_path = sorted(glob.glob("C:/Users/Q610267/source/repos/pdms_one_csv_streamwriter_modification_v2/pdms_one_csv_streamwriter_modification_v2/*.save"))[0]
        # load this model
        model = joblib.load(model_path)
        # read data from filepath
        new_data = load_data(self.filepath)
        new_data = new_data.fillna(method="ffill")
        # select feature
        X_v = new_data[['fCurrentScaled']]
        # predict label of this data
        predict = model.predict(X_v)
        # if the number of label 1 greater than 50%, print 'Yes', otherwise 'No'
        if sum(i == 1 for i in predict) / len(X_v) * 100 > 50:
            print('Yes')
        else:
            print('No')

        return print