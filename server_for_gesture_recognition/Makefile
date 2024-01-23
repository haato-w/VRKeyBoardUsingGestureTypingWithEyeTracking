build:
	docker-compose build

# コンテナを作成して立ち上げる
# コンテナが既に作成されている場合はexecされる
up:
	docker-compose up -d
	docker-compose exec python3 bash

# コンテナを止める　消さない
down:
	docker-compose stop

clean:
	docker-compose stop
	docker-compose down

# コンテナ内で実行する
# コンテナ内でライブラリ群をインストールする
install:
	pip3 install -r requirements.txt
