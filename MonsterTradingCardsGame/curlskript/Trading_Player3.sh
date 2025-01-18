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
token_weisshaus=$(curl -s -X POST http://localhost:10001/api/auth/login --header "Content-Type: application/json" -d "{\"Username\":\"weisshaus\", \"Password\":\"gerhard\"}" | grep -o '"token":"[^"]*' | grep -o '[^"]*$')
echo "$token_weisshaus"

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi

echo "2) show list cards of user to extract id for offering a trade and one for accepting it"

response=$(curl -s -X GET http://localhost:10001/api/cards \
  --header "Content-Type: application/json" \
  --header "Authorization: $token_weisshaus")

echo "$response"

ids=$(echo "$response" | grep -o '"Id":"[^"]*"' | tail -2 | awk -F':' '{print $2}' | tr -d '"')

offered_id=$(echo $ids | awk '{print $1}')
accepting_id=$(echo $ids | awk '{print $2}')

echo "Offered ID: $offered_id"
echo "Accepting ID: $accepting_id"

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi

echo "3) Offer a trade"
curl -i -X POST http://localhost:10001/api/tradings/ --header "Content-Type: application/json" --header "Authorization: $token_weisshaus" -d "{\"CardId\":\"$offered_id\", \"CardType\":\"Monster\" , \"MinDmg\":10, \"AutomaticAccept\":true}"

response=$(curl -i -X GET http://localhost:10001/api/tradings/ --header "Content-Type: application/json" --header "Authorization: $token_weisshaus")

echo "$response"

trading_id=$(echo "$response" | grep -o '"Id":"[^"]*"' | tail -1 | awk -F':' '{print $2}' | tr -d '"')

echo "$trading_id"

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi

echo "4) Accept a trade"
curl -i -X POST http://localhost:10001/api/tradings/$trading_id --header "Content-Type: application/json" --header "Authorization: $token_weisshaus" -d "{\"CardId\":\"$accepting_id\"}"


#curl -i -X DELETE http://localhost:10001/api/tradings/db2f49cd-9c59-4851-8846-c7ed0d02ec52  --header "Content-Type: application/json" --header "Authorization: $token_weisshaus"