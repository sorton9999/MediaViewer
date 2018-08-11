pipeline {
  agent any
  stages {
    stage('Bring forward ver2 code') {
      steps {
        build(job: 'build version', propagate: true, quietPeriod: 1, wait: true)
      }
    }
  }
}