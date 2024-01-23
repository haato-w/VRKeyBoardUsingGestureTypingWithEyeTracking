# ubuntu baseはpythonとpipを入れるのが手間かかるため良くない
# FROM ubuntu:23.10
# pip installのエラー回避のために少し古いpythonを入れる
# python3.8でライブラリをinstallできた
FROM python:3.8.18
USER root

RUN apt-get update
RUN apt-get -y install locales && \
    localedef -f UTF-8 -i ja_JP ja_JP.UTF-8
ENV LANG ja_JP.UTF-8
ENV LANGUAGE ja_JP:ja
ENV LC_ALL ja_JP.UTF-8
ENV TZ JST-9
ENV TERM xterm

# 一応入れておくツール
RUN apt-get install -y vim less
RUN apt-get install -y curl

RUN pip3 install --upgrade pip
# 他のライブラリとの相性が悪いためエラーが出るのでアップグレードしない
# RUN pip3 install --upgrade setuptools
