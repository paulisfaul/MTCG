!/bin/sh

pauseFlag=0
for arg in "$@"; do
    if [ "$arg" == "pause" ]; then
        pauseFlag=1 
        break
    fi
done

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi

echo "1) Login"

token_kienboec=$(curl -s -X POST http://localhost:10001/api/auth/login --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}" | grep -o '"token":"[^"]*' | grep -o '[^"]*$')
echo "$token_kienboec"

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi

echo "2) Scoreboard"

curl -i -X GET http://localhost:10001/api/scoreboard --header "Authorization: $token_kienboec"

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi

echo "3) Stats"

curl -i -X GET http://localhost:10001/api/stats --header "Authorization: $token_kienboec"

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi

echo "4) Battle"

curl -i -X POST http://localhost:10001/api/battle --header "Authorization: $token_kienboec"


