# MA-USV
해상 전장 상황에서의 강화학습 기반의 다중 에이전트 시뮬레이터 구현

## 버전 정보
* python 3.8.x
* pytorch 1.13.0
* Unity 2022.1.21f1
* ML-Agents 2.3.0-exp.3
* ML-Agents extensions 0.6.1-preview

## 학습 명령어
<pre><code>mlagents-learn "하이퍼파라미터 yaml 파일 경로" --env="빌드파일 경로" --run-id="저장할 모델 파일 경로" --num-envs="분산학습 개수" --no-graphics</code></pre>

## 알고리즘
* MA-POCA
* QMIX (추후 추가)

## 폴더 구성
> Resources
>   > Agent&Target
> 
>   > Bullet
>
>   > Model

> Scenes

> Scripts