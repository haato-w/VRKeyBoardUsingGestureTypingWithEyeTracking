'''

You can modify the parameters, return values and data structures used in every function if it conflicts with your
coding style or you want to accelerate your code.

You can also import packages you want.

But please do not change the basic structure of this file including the function names. It is not recommended to merge
functions, otherwise it will be hard for TAs to grade your code. However, you can add helper function if necessary.

'''

from flask import Flask, request
from flask import render_template
import time
import json

# IMPORTS
from sklearn.metrics.pairwise import euclidean_distances
from scipy.interpolate import interp1d
import numpy as np

import datetime


app = Flask(__name__)

# PRE-PROCESSING
# Number of sample points
num_sample_points = 100
# Calculate 100 evenly spaced numbers between 0 and 1
evenly_spaced_100_numbers = np.linspace(0, 1, num_sample_points)
# Calculate alphas for location score
alphas = np.zeros((num_sample_points))
mid_point = num_sample_points // 2
for i in range(mid_point):
    x = i/2450
    alphas[mid_point - i - 1], alphas[mid_point + i] = x, x

# Centroids of 26 keys
# centroids_X = [50, 205, 135, 120, 100, 155, 190, 225, 275, 260, 295, 330, 275, 240, 310, 345, 30, 135, 85, 170, 240, 170, 65, 100, 205, 65]
# centroids_Y = [85, 120, 120, 85, 50, 85, 85, 85, 50, 85, 85, 85, 120, 120, 50, 50, 50, 50, 85, 50, 50, 120, 50, 120, 50, 120]
centroids_X = [99,419,280,245,193,318,388,457,546,527,598,669,567,491,619,692,47,265,173,335,474,350,121,210,405,138]
centroids_Y = [96,156,156,96,37,96,96,96,37,96,96,96,156,156,37,37,37,37,96,37,37,156,37,156,37,156]

# Pre-process the dictionary and get templates of 10000 words
words, probabilities = [], {}
template_points_X, template_points_Y = [], []

# file = open('words_10000.txt')
# content = file.read()
# file.close()
# content = content.split('\n')
# for line in content:
#     line = line.split('\t')
#     words.append(line[0])
#     probabilities[line[0]] = float(line[2])
#     template_points_X.append([])
#     template_points_Y.append([])
#     for c in line[0]:
#         template_points_X[-1].append(centroids_X[ord(c) - 97])
#         template_points_Y[-1].append(centroids_Y[ord(c) - 97])


# 自前のコーパスの読み込み
content = []
corpus_fname = "corpus.txt"
# corpus_fname = "integrated_corpus.txt"
with open(corpus_fname, "r", encoding="utf-8") as f:
    while True:
      line = f.readline()
      if not line: break
      content.append(line.rstrip('\n'))

# print(content)

for line in content:
    words.append(line)
    # probabilities[line[0]] = float(line[2])
    template_points_X.append([])
    template_points_Y.append([])
    for c in line:
        template_points_X[-1].append(centroids_X[ord(c) - 97])
        template_points_Y[-1].append(centroids_Y[ord(c) - 97])



def generate_sample_points(points_X, points_Y):
    '''Generate 100 sampled points for a gesture.

    In this function, we should convert every gesture or template to a set of 100 points, such that we can compare
    the input gesture and a template computationally.

    :param points_X: A list of X-axis values of a gesture.
    :param points_Y: A list of Y-axis values of a gesture.

    :return:
        sample_points_X: A list of X-axis values of a gesture after sampling, containing 100 elements.
        sample_points_Y: A list of Y-axis values of a gesture after sampling, containing 100 elements.
    '''
    sample_points_X, sample_points_Y = [], []
    # TODO: Start sampling (12 points)

    # Calculate the euclidean distance between consecutive points
    distance = np.sqrt(np.ediff1d(points_X, to_begin=0) ** 2 + np.ediff1d(points_Y, to_begin=0) ** 2)
    # Calculate the cumulative distance
    cumulative_distance = np.cumsum(distance)
    # Normalize the cumulative distance between 0 and 1
    total_distance = cumulative_distance[-1]
    # print('total_distance: ', total_distance)
    cumulative_distance_norm = cumulative_distance / total_distance
    # if total_distance == 0:
    #     print(cumulative_distance)
    #     print(distance)
    #     print(cumulative_distance_norm)

    # Interpolate numbers into 1-D space for both X and Y
    interp1d_X = interp1d(cumulative_distance_norm, points_X, kind='linear')
    interp1d_Y = interp1d(cumulative_distance_norm, points_Y, kind='linear')

    # Create the sample points for X and Y
    sample_points_X, sample_points_Y = interp1d_X(evenly_spaced_100_numbers), interp1d_Y(evenly_spaced_100_numbers)
    return sample_points_X, sample_points_Y


# Pre-sample every template
template_sample_points_X, template_sample_points_Y = [], []
# for i in range(10000):
for i in range(len(template_points_X)):
    X, Y = generate_sample_points(template_points_X[i], template_points_Y[i])
    template_sample_points_X.append(X)
    template_sample_points_Y.append(Y)

# Normalize every template
L = 200
# Calculate scaling factor s
templates_width = np.max(template_sample_points_X, axis=1) - np.min(template_sample_points_X, axis=1)
templates_height = np.max(template_sample_points_Y, axis=1) - np.min(template_sample_points_Y, axis=1)
s = L / np.maximum(1, np.max(np.array([templates_width, templates_height]), axis=0))

print('flag1')

# Scale the points
scaling_matrix = np.diag(s)
# print('flag3')
# print(scaling_matrix)
# print('(', len(scaling_matrix), len(scaling_matrix[0]), ')')
# print('(', len(template_sample_points_X), len(template_sample_points_X[0]), ')')
# print(type(template_sample_points_X[0][0]))
# print(template_sample_points_X)
scaled_template_points_X = np.matmul(scaling_matrix, template_sample_points_X)
print('flag4')
scaled_template_points_Y = np.matmul(scaling_matrix, template_sample_points_Y)

print('flag2')

# Calculate translation factor tx and ty
scaled_template_centroid_X, scaled_template_centroid_Y = np.mean(scaled_template_points_X, axis=1), np.mean(scaled_template_points_Y, axis=1)
tx, ty = 0 - scaled_template_centroid_X, 0 - scaled_template_centroid_Y

# Translate the points
translation_matrix_X = np.reshape(tx, (-1, 1))
translation_matrix_Y = np.reshape(ty, (-1, 1))
normalized_template_sample_points_X = translation_matrix_X + scaled_template_points_X
normalized_template_sample_points_Y = translation_matrix_Y + scaled_template_points_Y


def do_pruning(gesture_points_X, gesture_points_Y, template_sample_points_X, template_sample_points_Y):
    '''Do pruning on the dictionary of 10000 words.

    In this function, we use the pruning method described in the paper (or any other method you consider it reasonable)
    to narrow down the number of valid words so that the ambiguity can be avoided to some extent.

    :param gesture_points_X: A list of X-axis values of input gesture points, which has 100 values since we have
        sampled 100 points.
    :param gesture_points_Y: A list of Y-axis values of input gesture points, which has 100 values since we have
        sampled 100 points.
    :param template_sample_points_X: 2D list, containing X-axis values of every template (10000 templates in total).
        Each of the elements is a 1D list and has the length of 100.
    :param template_sample_points_Y: 2D list, containing Y-axis values of every template (10000 templates in total).
        Each of the elements is a 1D list and has the length of 100.

    :return:
        valid_words: A list of valid words after pruning.
        valid_probabilities: The corresponding probabilities of valid_words.
        valid_template_sample_points_X: 2D list, the corresponding X-axis values of valid_words. Each of the elements
            is a 1D list and has the length of 100.
        valid_template_sample_points_Y: 2D list, the corresponding Y-axis values of valid_words. Each of the elements
            is a 1D list and has the length of 100.
    '''
    valid_words, valid_template_sample_points_X, valid_template_sample_points_Y = [], [], []
    # TODO: Set your own pruning threshold
    # threshold = 30
    start_threshold = 15
    end_threshold = 40
    # TODO: Do pruning (12 points)

    # Create numpy array for gesture start and end point [[x, y]]
    gesture_start_point = np.array([gesture_points_X[0], gesture_points_Y[0]])
    gesture_end_point = np.array([gesture_points_X[-1], gesture_points_Y[-1]])

    # Number of templates
    num_templates = len(template_sample_points_X)
    # Gather the start points and end points of templates in a numpy matrix [[x1, y1], [x2, y2], ..., [xn, yn]]
    template_start_points = np.array([[template_sample_points_X[i][0], template_sample_points_Y[i][0]] for i in range(num_templates)])
    template_end_points = np.array([[template_sample_points_X[i][-1], template_sample_points_Y[i][-1]] for i in range(num_templates)])

    # Calculate distances between start points of gesture and templates and end points of gesture and templates
    start_distances = euclidean_distances(np.reshape(gesture_start_point, (1, -1)), template_start_points)[0]
    end_distances = euclidean_distances(np.reshape(gesture_end_point, (1, -1)), template_end_points)[0]

    # Get indices whose start + end distances are less than the threshold
    # valid_indices = np.where((start_distances + end_distances) < threshold)[0]
    # 最後の座標の剪定条件をゆるくする
    valid_indices = np.where(((start_distances < start_threshold) & (end_distances < end_threshold)))[0]

    # Gather valid template sample points and valid words using the valid indices
    valid_template_sample_points_X = np.array(template_sample_points_X)[valid_indices]
    valid_template_sample_points_Y = np.array(template_sample_points_Y)[valid_indices]
    valid_words = [words[valid_index] for valid_index in valid_indices]

    return valid_indices, valid_words, valid_template_sample_points_X, valid_template_sample_points_Y


def get_shape_scores(valid_indices, gesture_sample_points_X, gesture_sample_points_Y, valid_template_sample_points_X, valid_template_sample_points_Y):
    '''Get the shape score for every valid word after pruning.

    In this function, we should compare the sampled input gesture (containing 100 points) with every single valid
    template (containing 100 points) and give each of them a shape score.

    :param gesture_sample_points_X: A list of X-axis values of input gesture points, which has 100 values since we
        have sampled 100 points.
    :param gesture_sample_points_Y: A list of Y-axis values of input gesture points, which has 100 values since we
        have sampled 100 points.
    :param valid_template_sample_points_X: 2D list, containing X-axis values of every valid template. Each of the
        elements is a 1D list and has the length of 100.
    :param valid_template_sample_points_Y: 2D list, containing Y-axis values of every valid template. Each of the
        elements is a 1D list and has the length of 100.

    :return:
        A list of shape scores.
    '''
    shape_scores = []
    # TODO: Set your own L
    L = 200
    # Calculate scaling factor s
    gesture_width = np.max(gesture_sample_points_X) - np.min(gesture_sample_points_X)
    gesture_height = np.max(gesture_sample_points_Y) - np.min(gesture_sample_points_Y)
    s = L / max(gesture_width, gesture_height, 1)

    # Scale the points
    scaling_matrix = np.array([[s, 0],
                               [0, s]])
    old_gesture_points = np.array([gesture_sample_points_X,
                                   gesture_sample_points_Y])
    scaled_gesture_points = np.matmul(scaling_matrix, old_gesture_points)

    # Calculate translation factor tx and ty
    scaled_gesture_centroid_X, scaled_gesture_centroid_Y = np.mean(scaled_gesture_points[0]), np.mean(scaled_gesture_points[1])
    tx, ty = 0 - scaled_gesture_centroid_X, 0 - scaled_gesture_centroid_Y

    # Translate the points
    translation_matrix = np.array([[tx],
                                   [ty]])
    normalized_gesture_sample_points = translation_matrix + scaled_gesture_points

    # TODO: Calculate shape scores (12 points)

    valid_normalized_template_sample_points_X = normalized_template_sample_points_X[valid_indices]
    valid_normalized_template_sample_points_Y = normalized_template_sample_points_Y[valid_indices]

    # Calculate (xi - xj)^2
    x_ = (valid_normalized_template_sample_points_X - np.reshape(normalized_gesture_sample_points[0], (1, -1))) ** 2
    # Calculate (yi - yj)^2
    y_ = (valid_normalized_template_sample_points_Y - np.reshape(normalized_gesture_sample_points[1], (1, -1))) ** 2
    # Calculate square root of (xi - xj)^2 + (yi - yj)^2
    distances = (x_ + y_) ** 0.5
    # Calculate shape scores as mean of distances
    shape_scores = np.sum(distances, axis=1) / num_sample_points

    return shape_scores


def get_location_scores(gesture_sample_points_X, gesture_sample_points_Y, valid_template_sample_points_X, valid_template_sample_points_Y):
    '''Get the location score for every valid word after pruning.

    In this function, we should compare the sampled user gesture (containing 100 points) with every single valid
    template (containing 100 points) and give each of them a location score.

    :param gesture_sample_points_X: A list of X-axis values of input gesture points, which has 100 values since we
        have sampled 100 points.
    :param gesture_sample_points_Y: A list of Y-axis values of input gesture points, which has 100 values since we
        have sampled 100 points.
    :param template_sample_points_X: 2D list, containing X-axis values of every valid template. Each of the
        elements is a 1D list and has the length of 100.
    :param template_sample_points_Y: 2D list, containing Y-axis values of every valid template. Each of the
        elements is a 1D list and has the length of 100.

    :return:
        A list of location scores.
    '''
    location_scores = []
    radius = 15
    # TODO: Calculate location scores (12 points)

    # Initialize location scores
    location_scores = np.zeros((len(valid_template_sample_points_X)))
    # Create a list of gesture points [[xi, yi]]
    gesture_points = [[gesture_sample_points_X[j], gesture_sample_points_Y[j]] for j in range(num_sample_points)]

    # For each template
    for i in range(len(valid_template_sample_points_X)):
        # Create a list of template points
        template_points = [[valid_template_sample_points_X[i][j], valid_template_sample_points_Y[i][j]] for j in range(num_sample_points)]
        # Calculate distance of each gesture point with each template point
        distances = euclidean_distances(gesture_points, template_points)
        # Find the distance of the closest gesture point to each template point
        template_gesture_min_distances = np.min(distances, axis=0)
        # Find the distance of the closest template point to each gesture point
        gesture_template_min_distances = np.min(distances, axis=1)
        # If any gesture point is not within the radius tunnel or any template point is not within the radius tunnel
        if np.any(gesture_template_min_distances > radius) or np.any(template_gesture_min_distances > radius):
            # Calculate delta as the distance of each gesture point with corresponding template point
            deltas = np.diagonal(distances)
            # Calculate location score as sum of product of alpha and delta for each point
            location_scores[i] = np.sum(np.multiply(alphas, deltas))

    return location_scores


def get_integration_scores(shape_scores, location_scores):
    integration_scores = []
    # TODO: Set your own shape weight
    shape_coef = 0.5 # 小さい方が甘く、大きい方が厳しい
    # TODO: Set your own location weight
    location_coef = 1 - shape_coef
    integration_scores = shape_coef * shape_scores + location_coef * location_scores
    return integration_scores


def get_best_word(valid_words, integration_scores):
    '''Get the best word.

    In this function, you should select top-n words with the highest integration scores and then use their corresponding
    probability (stored in variable "probabilities") as weight. The word with the highest weighted integration score is
    exactly the word we want.

    :param valid_words: A list of valid words.
    :param integration_scores: A list of corresponding integration scores of valid_words.
    :return: The most probable word suggested to the user.
    '''
    best_word = 'the'
    # TODO: Set your own range.
    n = 3
    # TODO: Get the best word (12 points)

    # Find indices having the minimum score
    min_score = np.min(np.array(integration_scores))
    min_score_indices = np.where(integration_scores == min_score)[0]
    # Create a list of words having minimum scores
    best_words = [valid_words[min_score_index] for min_score_index in min_score_indices]
    # Return the best words separated by space
    return ' '.join(best_words)


# 確率が上位３位までの単語を出力を選ぶ
def get_best_top_3_accuracy_words(valid_words: list, integration_scores: list) -> str:
    sorted_score = np.sort(np.array(integration_scores))

    min_indices = np.where(integration_scores == sorted_score[0])[0]
    second_indices = np.where(integration_scores == sorted_score[1])[0] if len(sorted_score) >= 2 else []
    third_indices = np.where(integration_scores == sorted_score[2])[0] if len(sorted_score) >= 3 else []
    
    best_words = [valid_words[min_score_index] for min_score_index in min_indices]
    second_words = [valid_words[second_score_index] for second_score_index in second_indices]
    third_words = [valid_words[third_score_index] for third_score_index in third_indices]

    res = ' '.join(best_words + second_words + third_words)

    return res


@app.route("/gesture-recognition")
def init():
    return render_template('index.html')


@app.route('/shark2', methods=['POST'])
def shark2():

    start_time = time.time()
    data = json.loads(request.get_data())
    data = data['data']
    print(data)

    gesture_points_X = []
    gesture_points_Y = []
    for i in range(len(data)):
        gesture_points_X.append(data[i]['x'])
        gesture_points_Y.append(data[i]['y'])
    # gesture_points_X = [gesture_points_X]
    # gesture_points_Y = [gesture_points_Y]

    # 一文字だけの入力に対応させる
    # 最も近い文字を出力する
    N = 25
    if len(data) < N:
        min_gosa = 10**10
        best_alpha = 'a'
        for ind in range(len(centroids_X)):
            gosa = 0
            for g_x, g_y in zip(gesture_points_X, gesture_points_Y):
                gosa += abs(g_x - centroids_X[ind]) ** 2 + abs(g_y - centroids_Y[ind]) ** 2
            if gosa < min_gosa:
                min_gosa = gosa
                best_alpha = chr(ind + 97)
        
        end_time = time.time()
        return '{"best_word":"' + best_alpha + '", "elapsed_time":"' + str(round((end_time - start_time) * 1000, 5)) + 'ms"}'

    gesture_sample_points_X, gesture_sample_points_Y = generate_sample_points(gesture_points_X, gesture_points_Y)

    valid_indices, valid_words, valid_template_sample_points_X, valid_template_sample_points_Y = do_pruning(gesture_points_X, gesture_points_Y, template_sample_points_X, template_sample_points_Y)

    best_word = "Word not found"
    if len(valid_words) != 0:
        shape_scores = get_shape_scores(valid_indices, gesture_sample_points_X, gesture_sample_points_Y, valid_template_sample_points_X, valid_template_sample_points_Y)

        location_scores = get_location_scores(gesture_sample_points_X, gesture_sample_points_Y, valid_template_sample_points_X, valid_template_sample_points_Y)

        integration_scores = get_integration_scores(shape_scores, location_scores)

        # best_word = get_best_word(valid_words, integration_scores)

        best_word = get_best_top_3_accuracy_words(valid_words, integration_scores)

    end_time = time.time()

    return '{"best_word":"' + best_word + '", "elapsed_time":"' + str(round((end_time - start_time) * 1000, 5)) + 'ms"}'


@app.route('/registerData', methods=['POST'])
def register_data():
    data = json.loads(request.get_data())
    input_method = data['input_method'] # data変数を入れ替える前に受け取る
    # data = data['data']
    input_data = data['data']
    print('--------------------------------------------')
    print('--------------------------------------------')
    print('input_method: ', input_method)
    print('received data num: ', len(input_data))
    print(input_data)
    print('--------------------------------------------')
    print('--------------------------------------------')

    RECORD_DIR = './acquired_data/'
    dt_now = datetime.datetime.now()

    f_name = str(dt_now.year) + '_' + str(dt_now.month) + '_' + str(dt_now.day) + '_' + str(dt_now.hour) + '_' + str(dt_now.minute) + '_' + str(dt_now.second)

    with open(RECORD_DIR + f_name + '.json', 'w') as f:
        json.dump(data, f, indent=2)

    return '{"flag: true"}'



@app.route('/get_score', methods=['POST'])
def get_score():

    data = json.loads(request.get_data())
    target_word = data['word']
    data = data['data']
    print(data)

    gesture_points_X = []
    gesture_points_Y = []
    for i in range(len(data)):
        gesture_points_X.append(data[i]['x'])
        gesture_points_Y.append(data[i]['y'])
    # gesture_points_X = [gesture_points_X]
    # gesture_points_Y = [gesture_points_Y]

    gesture_sample_points_X, gesture_sample_points_Y = generate_sample_points(gesture_points_X, gesture_points_Y)

    # valid_indices, valid_words, valid_template_sample_points_X, valid_template_sample_points_Y = do_pruning(gesture_points_X, gesture_points_Y, template_sample_points_X, template_sample_points_Y)
    # ここで単語のインデックスを出す
    valid_indices = np.array([words.index(target_word)])
    valid_template_sample_points_X = np.array(template_sample_points_X)[valid_indices]
    valid_template_sample_points_Y = np.array(template_sample_points_Y)[valid_indices]

    shape_scores = get_shape_scores(valid_indices, gesture_sample_points_X, gesture_sample_points_Y, [], [])
    print(shape_scores[0])
    print(type(shape_scores[0]))

    location_scores = get_location_scores(gesture_sample_points_X, gesture_sample_points_Y, valid_template_sample_points_X, valid_template_sample_points_Y)

    integration_scores = get_integration_scores(shape_scores, location_scores)

    return '{"shape_score":"' + str(shape_scores[0].astype(float)) + \
        '", "location_scores":"' + str(location_scores[0].astype(float)) + \
        '", "integration_scores":"' + str(integration_scores[0].astype(float)) + '"}'
    # return '{"shape_score":"' + str(shape_scores[0].astype(float)) + '"}'


if __name__ == "__main__":
    app.run(host='0.0.0.0', port=8080)

