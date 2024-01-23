
from flask import Flask, request
from flask import render_template
import time
import json

# IMPORTS
from sklearn.metrics.pairwise import euclidean_distances
from scipy.interpolate import interp1d
import numpy as np

app = Flask(__name__)


@app.route('/test_api', methods=['POST'])
def test_api():

    # start_time = time.time()
    # data = json.loads(request.get_data())

    # gesture_points_X = []
    # gesture_points_Y = []
    # for i in range(len(data)):
    #     gesture_points_X.append(data[i]['x'])
    #     gesture_points_Y.append(data[i]['y'])
    # # gesture_points_X = [gesture_points_X]
    # # gesture_points_Y = [gesture_points_Y]

    # gesture_sample_points_X, gesture_sample_points_Y = generate_sample_points(gesture_points_X, gesture_points_Y)

    # valid_indices, valid_words, valid_template_sample_points_X, valid_template_sample_points_Y = do_pruning(gesture_points_X, gesture_points_Y, template_sample_points_X, template_sample_points_Y)

    # best_word = "Word not found"
    # if len(valid_words) != 0:
    #     shape_scores = get_shape_scores(valid_indices, gesture_sample_points_X, gesture_sample_points_Y, valid_template_sample_points_X, valid_template_sample_points_Y)

    #     location_scores = get_location_scores(gesture_sample_points_X, gesture_sample_points_Y, valid_template_sample_points_X, valid_template_sample_points_Y)

    #     integration_scores = get_integration_scores(shape_scores, location_scores)

    #     best_word = get_best_word(valid_words, integration_scores)

    #     end_time = time.time()

    # return '{"best_word":"' + best_word + '", "elapsed_time":"' + str(round((end_time - start_time) * 1000, 5)) + 'ms"}'

    content_type = dict(request.headers)
    print(content_type)
    data = json.loads(request.get_data())
    print(data['data'])

    return '{"hello"}'


if __name__ == "__main__":
    app.run()
